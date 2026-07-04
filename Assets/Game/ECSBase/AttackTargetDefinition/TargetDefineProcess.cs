using System;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    public class TargetDefineProcess : IDisposable
    {
        private TargetDefineJob _job;
        private readonly float _hexEdgeLength;
        private readonly int _trianglesPerHexEdge;
        private readonly World _world;
        private readonly PlayerRelations _relations;
        private NativeArray<PlayerRelationsMask> _relationMasks;

        private readonly Stash<PlayerAffiliationComponent> _affiliationComponents;
        private readonly Stash<AttackTargetComponent> _attackTargets;
        private readonly Stash<PositionComponent> _positionComponents;
        private readonly Stash<HexCoordComponent> _hexCoordComponents;
        private readonly Stash<TargetSearchRadiusComponent> _targetSearchRadiusComponents;

        public TargetDefineProcess(INavigationMap map, IPlayersList playersList, PlayerRelations relations, World world)
        {
            _relations = relations;
            _world = world;

            _hexEdgeLength = map.HexEdgeLength;
            _trianglesPerHexEdge = map.TrianglesPerHexEdge;

            _affiliationComponents = _world.GetStash<PlayerAffiliationComponent>();
            _attackTargets = _world.GetStash<AttackTargetComponent>();
            _positionComponents = _world.GetStash<PositionComponent>();
            _hexCoordComponents = _world.GetStash<HexCoordComponent>();
            _targetSearchRadiusComponents = _world.GetStash<TargetSearchRadiusComponent>();

            _relationMasks = new NativeArray<PlayerRelationsMask>(playersList.Count, Allocator.Persistent);

            _job = new()
            {
                HexEdgeLength = _hexEdgeLength,
                TrianglesPerEdge = _trianglesPerHexEdge,
                EnemiesMask = _relationMasks
            };
        }

        public JobHandle Launch(Filter filter)
        {
            var dirtyFlag = false;
            foreach (var entity in filter)
            {
                if (!_attackTargets.Has(entity))
                {
                    _attackTargets.Add(entity);
                    dirtyFlag = true;
                }
            }

            if (dirtyFlag)
                _world.Commit();
            

            // WARNING: Native stashes and filters exists only for one frame!
            var nativeFilter = filter.AsNative();
            _job.Filter = nativeFilter;
            _job.AffiliationsStash = _affiliationComponents.AsNative();
            _job.AttackTargets = _attackTargets.AsNative();
            _job.HexCoordComponents = _hexCoordComponents.AsNative();
            _job.PositionComponents = _positionComponents.AsNative();
            _job.TargetSearchRadius = _targetSearchRadiusComponents.AsNative();
            UpdateRelationsMask();

            return _job.Schedule(nativeFilter.length, 32);
        }

        public void Dispose()
        {
            _relationMasks.Dispose();
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
