using System.Collections.Generic;
using VContainer;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Jobs.LowLevel;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TransformsSyncSystem : ILateSystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<TransformComponent> _transforms;
        private Stash<PositionComponent> _positions;
        private Stash<RotationComponent> _rotation;
        private int _currentSyncTableCapacity;
        private NativeParallelHashMap<int, RigidTransform> _syncTable;
        private readonly TransformAccessManager _transformAccessManager;
        private const int DEFAULT_CAPACITY = 64;

        [Inject]
        public TransformsSyncSystem(TransformAccessManager transformAccessManager)
        {
            _transformAccessManager = transformAccessManager;

            _syncTable = new(DEFAULT_CAPACITY, Allocator.Persistent);
            _currentSyncTableCapacity = DEFAULT_CAPACITY;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<TransformComponent>()
                .With<TransformUpdatedTag>()
                .Build();

            _transforms = World.GetStash<TransformComponent>();
            _positions = World.GetStash<PositionComponent>();
            _rotation = World.GetStash<RotationComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            var nativeFilter = _filter.AsNative();
            var elementsCount = nativeFilter.length;
            if (elementsCount > _currentSyncTableCapacity)
                IncreaseSyncTableCapacity(elementsCount);

            _syncTable.Clear();
            var prepareDataJob = new PrepareTransformsDataJob()
            {
                Entities = nativeFilter,
                Transforms = _transforms.AsNative(),
                Positions = _positions.AsNative(),
                Rotations = _rotation.AsNative(),
                KeysToIndexMap = _transformAccessManager.KeysMap.AsReadOnly(),
                SyncData = _syncTable.AsParallelWriter()
            };
            var prepareDataHandle = prepareDataJob.Schedule(nativeFilter.length, 64, World.JobHandle);

            var syncJob = new TransformSyncParallelJob()
            {
                SyncData = _syncTable.AsReadOnly(),
            };
            World.JobHandle = syncJob.ScheduleByRef(_transformAccessManager.TransformsArray, prepareDataHandle);
        }

        public void Dispose()
        {
            _syncTable.Dispose();
        }

        private void IncreaseSyncTableCapacity(int requiredCapacity)
        {
            while (_currentSyncTableCapacity < requiredCapacity)
                _currentSyncTableCapacity *= 2;

            var newTable = new NativeParallelHashMap<int, RigidTransform>(_currentSyncTableCapacity, Allocator.Persistent);
            foreach (var kvp in _syncTable)
            {
                newTable.Add(kvp.Key, kvp.Value);
            }

            _syncTable.Dispose();
            _syncTable = newTable;
        }

        [BurstCompile]
        private struct PrepareTransformsDataJob : IJobParallelFor
        {
            [ReadOnly] public NativeFilter Entities;
            [ReadOnly] public NativeStash<TransformComponent> Transforms;
            [ReadOnly] public NativeParallelHashMap<int,int>.ReadOnly KeysToIndexMap;
            [ReadOnly] public NativeStash<PositionComponent> Positions;
            [ReadOnly] public NativeStash<RotationComponent> Rotations;

            [WriteOnly]public NativeParallelHashMap<int, RigidTransform>.ParallelWriter SyncData;

            public void Execute(int index)
            {
                var entity = Entities[index];
                var key = Transforms.Get(entity).Key;
                if (KeysToIndexMap.TryGetValue(key, out var transformIndex))
                {
                    SyncData.TryAdd(transformIndex, new(Rotations.Get(entity).Value, Positions.Get(entity).Value));
                }       
                #if UNITY_EDITOR
                else
                {
                    UnityEngine.Debug.LogWarning($"transform key {key} not found");
                }
                #endif
            }
        }

        [BurstCompile]
        private struct TransformSyncParallelJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeParallelHashMap<int, RigidTransform>.ReadOnly SyncData;

            public void Execute(int index, TransformAccess transform)
            {
               if (SyncData.TryGetValue(index, out var data))
                {                    
                    transform.SetPositionAndRotation(data.pos, data.rot);
                }
                    
            }
        }
    }
}