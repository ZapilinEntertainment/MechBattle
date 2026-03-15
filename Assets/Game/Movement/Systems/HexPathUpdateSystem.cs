using UnityEngine;
using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    // TODO: divide into 3 systems, which one of different group
    // 1) remove components of invalidized paths - clear group
    // 2) sets path keys for existing paths - fixed update
    // 3) check if paths are calculated for awaiting paths - after calculation system
    public class HexPathUpdateSystem : ISystem
    {
        public World World { get; set; }

        private readonly NavigationPathsList _pathsList;
        private NavigationMap _map;

        private Filter _invalidPathsFilter;
        private Filter _noPathEntitiesFilter;
        private Filter _calculatingPathsFilter;

        private Stash<MoveTargetComponent> _moveTargets;
        private Stash<PositionComponent> _positions;
        private Stash<NavHexPathComponent> _hexPaths;
        private Stash<InvalidHexPathTag> _invalidTags;
        private Stash<CalculatingHexPathComponent> _calculatingComponents;

        public HexPathUpdateSystem(NavigationPathsList pathsList)
        {
            _pathsList = pathsList;
        }

        public void OnAwake()
        {
            _moveTargets = World.GetStash<MoveTargetComponent>();
            _positions = World.GetStash<PositionComponent>();
            _hexPaths = World.GetStash<NavHexPathComponent>();
            _invalidTags = World.GetStash<InvalidHexPathTag>();
            _calculatingComponents = World.GetStash<CalculatingHexPathComponent>();

            _invalidPathsFilter = World.Filter
                .With<InvalidHexPathTag>()
                .Build();

            _noPathEntitiesFilter = World.Filter
                .With<MoveTargetComponent>()
                .Without<NavHexPathComponent>()
                .Without<CalculatingHexPathComponent>()
                .Build();

            _calculatingPathsFilter = World.Filter
                .With<CalculatingHexPathComponent>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _invalidPathsFilter)
            {
                _hexPaths.Remove(entity);
                _invalidTags.Remove(entity);
            }

            if (!_map.IsInitialized)
                return;

            foreach (var entity in _noPathEntitiesFilter)
            {
                var entityPos = _positions.Get(entity).Value;
                var entityHexPos = _map.WorldToHex(entityPos);

                var targetPos = _moveTargets.Get(entity).Value;
                var targetHexPos = _map.WorldToHex(targetPos);

                if (math.all(entityHexPos == targetHexPos))
                {
                    //same hex, no calculation needed
                    _hexPaths.Set(entity, new() { IsEmpty = true});
                    continue;
                }

                if (_pathsList.TryGetPathId(entityHexPos, targetHexPos, out var pathId))
                {
                    // path already calculated
                    _hexPaths.Set(entity, new() { PathId = pathId, StepIndex = 0 });
                }
                else
                {
                    // no path found, wait until being calculated
                    _pathsList.RequestPathBuilding(entityHexPos, targetHexPos);
                    _calculatingComponents.Set(entity, new(entityHexPos, targetHexPos));
                }
            }

            foreach (var entity in _calculatingPathsFilter)
            {
                var pathData = _calculatingComponents.Get(entity);
                if (_pathsList.TryGetPathId(pathData.CombinedValue, out var pathId))
                {
                    _hexPaths.Set(entity, new() { PathId = pathId, StepIndex = 0 });
                    _calculatingComponents.Remove(entity);
                }
            }
        }

        public void Dispose() { }
    }
}
