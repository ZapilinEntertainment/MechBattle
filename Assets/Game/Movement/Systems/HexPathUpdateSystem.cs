using UnityEngine;
using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Movement;

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
        private readonly INavigationMap _map;
        private readonly HexPathSearcher _hexPathSearcher;

        private Filter _noPathEntitiesFilter;
        private Filter _calculatingPathsFilter;

        private Stash<MoveTargetComponent> _moveTargets;
        private Stash<PositionComponent> _positions;
        private Stash<RegularHexPathComponent> _regularHexPaths;
        private Stash<TransitionHexPathComponent> _transitionHexPaths;
        private Stash<CalculatingHexPathComponent> _calculatingComponents;
        private Stash<HexPathDefinedTag> _definedTags;

        public HexPathUpdateSystem(NavigationHexPathsList pathsList, INavigationMap map, HexPathSearcher hexPathSearcher)
        {
            _pathsList = pathsList;
            _map = map;
            _hexPathSearcher = hexPathSearcher;
        }

        public void OnAwake()
        {
            _moveTargets = World.GetStash<MoveTargetComponent>();
            _positions = World.GetStash<PositionComponent>();
            _regularHexPaths = World.GetStash<RegularHexPathComponent>();
            _transitionHexPaths = World.GetStash<TransitionHexPathComponent>();
            _calculatingComponents = World.GetStash<CalculatingHexPathComponent>();
            _definedTags = World.GetStash<HexPathDefinedTag>();
            

            _noPathEntitiesFilter = World.Filter
                .With<NavigationAgentComponent>()
                .With<MoveTargetComponent>()
                .Without<HexPathDefinedTag>()
                .Build();

            _calculatingPathsFilter = World.Filter
                .With<CalculatingHexPathComponent>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_map.IsInitialized)
                return;

            AssignHexPathComponents();
            CheckIfCalculatingPathsAreReady();            
        }

        public void Dispose() { }

        private void AssignHexPathComponents()
        {
            foreach (var entity in _noPathEntitiesFilter)
            {
                var checkResult = CheckForAvailablePaths(entity, out var searchResultData);

                switch (checkResult)
                {
                    // do nothing - no path needed:
                    //case HexPathSearchResult.PointsAreInSameHex:

                    case HexPathSearchResult.NoPathFound:
                        {
                            // no path found, wait until being calculated                           
                            _calculatingComponents.Set(entity, new(searchResultData));
                            break;
                        }
                    case HexPathSearchResult.PathFound:
                        {
                            // path already calculated
                            _regularHexPaths.Set(entity, new(searchResultData));
                            break;
                        }
                    case HexPathSearchResult.SingleEdgePass:
                        {
                            // only 1 edge transition needed
                            _transitionHexPaths.Set(entity, new(searchResultData.EndHex, searchResultData.ExitEdge));
                            break;
                        }
                }
                _definedTags.Add(entity);
                //Debug.Log(checkResult);
            }
        }

        private void CheckIfCalculatingPathsAreReady()
        {
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
                if (checkResult == HexPathSearchResult.NoPathFound)
                    continue;

                if (checkResult == HexPathSearchResult.PathFound)
                {
                    _regularHexPaths.Set(entity, new(searchResultData));
                    _calculatingComponents.Remove(entity);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning("this is not supposed to be!");
#endif
                    // do path request again:
                    _calculatingComponents.Remove(entity);
                    _definedTags.Remove(entity);
                }
            }
        }

        private HexPathSearchResult CheckForAvailablePaths(Entity entity, out HexPathSearchResultData searchResultData)
        {
            var entityPos = _positions.Get(entity).Value;
            var targetPos = _moveTargets.Get(entity).WorldPos;

            return _hexPathSearcher.TryGetShortestPath(
                entityPos,
                targetPos,
                out searchResultData);
        }
    }
}
