using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;
using Unity.Collections;
using Scellecs.Morpeh.Native;
using Unity.Mathematics;
using Unity.Jobs;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ChildPointsUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Filter _stampFilter;
        private Stash<ParentEntityComponent> _parents;
        private Stash<ChildTransformLastSyncStampComponent> _stampComponents;

        private Stash<TriangularPosComponent> _triangularPositions;
        private Stash<HexCoordComponent> _hexCoords;
        private Stash<LocalPositionComponent> _localPositions;
        private Stash<LocalRotationComponent> _localRotations;
        private Stash<PositionComponent> _positions;
        private Stash<RotationComponent> _rotations;

        private int _iterationNumber = 1;

        private NativeParallelHashSet<Entity> _handledEntities = new();
        private JobHandle _activeJobHandle;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<ParentEntityComponent>()
                .With<TransformUpdatedTag>()
                .With<LocalPositionComponent>()
                .With<LocalRotationComponent>()
                .Build();

            _stampFilter = World.Filter
                .With<ParentEntityComponent>()
                .With<TransformUpdatedTag>()
                .Without<ChildTransformLastSyncStampComponent>()
                .Build();

            _parents = World.GetStash<ParentEntityComponent>();
            _stampComponents = World.GetStash<ChildTransformLastSyncStampComponent>();

            _triangularPositions = World.GetStash<TriangularPosComponent>();
            _hexCoords = World.GetStash<HexCoordComponent>();
            _localPositions = World.GetStash<LocalPositionComponent>();
            _localRotations = World.GetStash<LocalRotationComponent>();
            _positions = World.GetStash<PositionComponent>();
            _rotations = World.GetStash<RotationComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;
            //UnityEngine.Debug.Log($"iteration {_iterationNumber}");

            if(_stampFilter.IsNotEmpty())
            {
                foreach (var entity in _stampFilter)
                {
                    _stampComponents.Add(entity);
                }
                World.Commit();
            }
            

            var nativeFilter = _filter.AsNative();
            var capacity = _handledEntities.IsCreated ? _handledEntities.Capacity : 0;

            if (capacity < nativeFilter.length)
            {
                _handledEntities.Dispose();
                _handledEntities = new(math.ceilpow2(nativeFilter.length), Allocator.Persistent);
            }
            else
            {
                _handledEntities.Clear();
            }

            var job = new ChildPointEntityUpdateJob()
            {
                Filter = nativeFilter,
                StampStash = _stampComponents.AsNative(),
                HandledEntities = _handledEntities,
                ParentStash = _parents.AsNative(),
                HexCoords = _hexCoords.AsNative(),
                TriangularPositions = _triangularPositions.AsNative(),
                LocalPositions = _localPositions.AsNative(),
                LocalRotations = _localRotations.AsNative(),
                Positions = _positions.AsNative(),
                Rotations = _rotations.AsNative(),
                IterationNumber = _iterationNumber
            };
            _activeJobHandle = job.Schedule();
            World.JobHandle = _activeJobHandle;

            _iterationNumber++;
        }

        public void Dispose() 
        {
            _activeJobHandle.Complete();
            _handledEntities.Dispose();
        }
    }
}