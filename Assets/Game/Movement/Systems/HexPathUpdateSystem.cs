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

        private readonly NavigationHexPathsList _pathsList;
        private NavigationMap _map;

        private Filter _invalidPathsFilter;
        private Filter _noPathEntitiesFilter;
        private Filter _calculatingPathsFilter;

        private Stash<MoveTargetComponent> _moveTargets;
        private Stash<PositionComponent> _positions;
        private Stash<NavHexPathComponent> _hexPaths;
        private Stash<InvalidHexPathTag> _invalidTags;
        private Stash<CalculatingHexPathComponent> _calculatingComponents;

        public HexPathUpdateSystem(NavigationHexPathsList pathsList)
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
                var checkResult = CheckForAvailablePaths(entity, out var searchResultData);

                switch (checkResult)
                {
                    case GetSuitablePathKeyCommand.HexPathSearchResult.PointsAreInSameHex:
                        {
                            _hexPaths.Set(entity, new() { IsEmpty = true});
                            break;
                        }
                    case GetSuitablePathKeyCommand.HexPathSearchResult.NoPathFound:
                        {
                            // no path found, wait until being calculated (request done inside command)
                            
                            _calculatingComponents.Set(entity, new(searchResultData));
                            break;
                        }
                     case GetSuitablePathKeyCommand.HexPathSearchResult.PathFound:
                        {
                            // path already calculated
                            _hexPaths.Set(entity, new() { PathId = searchResultData.PathId, StepIndex = 0 });
                            break;
                        }
                }
            }

            foreach (var entity in _calculatingPathsFilter)
            {
                var pathData = _calculatingComponents.Get(entity);
                if (pathData.UsedPathListVersion == _pathsList.Version)
                    continue;

                var startMayBeAccessible = pathData.StartEdgesMask.HasOverlapsWith(_pathsList.GetCalculatedEdgesMask(pathData.StartHex));
                var endMayBeAccessible = pathData.EndEdgesMask.HasOverlapsWith(_pathsList.GetCalculatedEdgesMask(pathData.EndHex));

                if (startMayBeAccessible & endMayBeAccessible == false)
                    continue;

                var checkResult = CheckForAvailablePaths(entity, out var searchResultData);
                if (checkResult == GetSuitablePathKeyCommand.HexPathSearchResult.NoPathFound)
                    continue;

                if (checkResult == GetSuitablePathKeyCommand.HexPathSearchResult.PathFound)
                {
                    _hexPaths.Set(entity, new() { PathId = searchResultData.PathId, StepIndex = 0 });
                    _calculatingComponents.Remove(entity);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning("this is not supposed to be!");
#endif

                    _hexPaths.Set(entity, new() { IsEmpty = true });
                    _calculatingComponents.Remove(entity);
                }

            }
        }

        public void Dispose() { }

        private GetSuitablePathKeyCommand.HexPathSearchResult CheckForAvailablePaths(Entity entity, out GetSuitablePathKeyCommand.HexPathSearchResultData searchResultData)
        {
            var entityPos = _positions.Get(entity).Value;
            var targetPos = _moveTargets.Get(entity).Value;

            return GetSuitablePathKeyCommand.TryGetClosestPath(
                entityPos,
                targetPos,
                _map,
                _pathsList,
                out searchResultData);
        }
    }
}
