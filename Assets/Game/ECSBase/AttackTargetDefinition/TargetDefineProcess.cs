using System;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    public class TargetDefineProcess : IDisposable
    {
        private TargetDefineJob _job;
        private NativeArray<PlayerRelationsMask> _relationMasks;
        private NativeList<Entity> _entitiesList;
        private JobHandle _activeJobHandle;
        private NativeParallelHashMap<IntTriangularPos, Entity> _entitiesMap;

        private readonly float _hexEdgeLength;
        private readonly World _world;
        private readonly PlayerRelations _relations;       

        private readonly Stash<PlayerAffiliationComponent> _affiliationComponents;
        private readonly Stash<AttackTargetComponent> _attackTargets;
        private readonly Stash<PositionComponent> _positionComponents;
        private readonly Stash<HexCoordComponent> _hexCoordComponents;
        private readonly Stash<TargetSearchRadiusComponent> _targetSearchRadiusComponents;

        private readonly Filter _movementCellsFilter;
        private readonly Stash<CellMovementDataComponent> _movementCells;
        private readonly Stash<CellEntityComponent> _cellComponents;
        private const Allocator ALLOCATOR = Allocator.Persistent;

        public TargetDefineProcess(
            INavigationMap map, 
            IPlayersList playersList, 
            IMovementCellsMap movementCellsMap,
            PlayerRelations relations, 
            World world)
        {
            _relations = relations;
            _world = world;

            _hexEdgeLength = map.HexEdgeLength;

            _affiliationComponents = _world.GetStash<PlayerAffiliationComponent>();
            _attackTargets = _world.GetStash<AttackTargetComponent>();
            _positionComponents = _world.GetStash<PositionComponent>();
            _hexCoordComponents = _world.GetStash<HexCoordComponent>();
            _targetSearchRadiusComponents = _world.GetStash<TargetSearchRadiusComponent>();

            _relationMasks = new NativeArray<PlayerRelationsMask>(playersList.Count, ALLOCATOR);     
            _entitiesList = new NativeList<Entity>(ALLOCATOR);
            _entitiesMap = new NativeParallelHashMap<IntTriangularPos, Entity>(128, ALLOCATOR);

            _movementCellsFilter = _world.Filter.With<CellMovementDataComponent>().Build();
            _movementCells = _world.GetStash<CellMovementDataComponent>();
            _cellComponents = _world.GetStash<CellEntityComponent>();

            _job = new()
            {
                HexEdgeLength = _hexEdgeLength,
                EnemiesMask = _relationMasks,
                TriangleHeight = map.TriangleHeight
            };
        }

        public JobHandle Launch(Filter filter)
        {
            _entitiesList.Clear();
            foreach (var entity in filter)
            {
                _attackTargets.Set(entity);
                _entitiesList.Add(entity);
            }

            _world.Commit();


            // WARNING: Native stashes and filters exists only for one frame!
            _job.Entities = _entitiesList;
            _job.AffiliationsStash = _affiliationComponents.AsNative();
            _job.AttackTargets = _attackTargets.AsNative();
            _job.HexCoordComponents = _hexCoordComponents.AsNative();
            _job.PositionComponents = _positionComponents.AsNative();
            _job.TargetSearchRadius = _targetSearchRadiusComponents.AsNative();

            _entitiesMap = FormTargetsMapCommand.Execute(
                _entitiesMap,
                ALLOCATOR,
                _movementCellsFilter,
                _movementCells,
                _cellComponents);
            _job.EntitiesMap = _entitiesMap;
            UpdateRelationsMask();

            _activeJobHandle = _job.Schedule(_entitiesList.Length, 32);
            return _activeJobHandle;
        }

        public async void Dispose()
        {
            if (!_activeJobHandle.IsCompleted)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"{nameof(_job)} is not yet completed");
#endif
                do
                {
                    await Awaitable.NextFrameAsync();
                }
                while (!_activeJobHandle.IsCompleted);
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"{nameof(_job)} finally stopped");
#endif
            }
#if UNITY_EDITOR
            try
            {
                FinalDispose();
            }
            catch
            {
                // ignore possible misses
            }
#else
            FinalDispose();
#endif
        }

        private void FinalDispose()
        {
            _relationMasks.Dispose();
            _entitiesList.Dispose();
            _entitiesMap.Dispose();
        }

        private void UpdateRelationsMask()
        {
            for (var i = 0; i < _relationMasks.Length; i++)
            {
                _relationMasks[i] = _relations.GetEnemiesMask(i);
            }
        }
    }
}
