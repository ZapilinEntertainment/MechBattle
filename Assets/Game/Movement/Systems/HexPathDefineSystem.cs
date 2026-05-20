using UnityEngine;
using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Movement;

namespace ZE.MechBattle.Ecs
{
    public class HexPathDefineSystem : ISystem
    {
        public World World { get; set; }

        private readonly HexPathsLRUBuffer _hexPathsList;
        private readonly INavigationMap _map;
        private readonly float _hexEdgeLength;
        private readonly HexDataAccessHandler _hexDataAccessHandler;
        private readonly HexPathSearcher _hexPathSearcher;

        private Filter _noPathEntitiesFilter;

        private Stash<MoveTargetComponent> _moveTargets;
        private Stash<HexCoordComponent> _hexCoords;
        private Stash<PositionComponent> _positions;
        private Stash<HexPathDefinedTag> _hexPathDefinedTag;
        private Stash<HexPathComponent> _hexPathComponents;
        private Stash<TriangularPosComponent> _triangularPosComponents;
        private Stash<MovementAwaitingComponent> _awaitingComponents;

        public HexPathDefineSystem(
            HexPathsLRUBuffer pathsList, 
            INavigationMap map, 
            HexPathSearcher hexPathsSearcher,
            HexDataAccessHandler hexDataAccessHandler)
        {
            _hexPathsList = pathsList;
            _map = map;
            _hexPathSearcher = hexPathsSearcher;
            _hexDataAccessHandler = hexDataAccessHandler;

            _hexEdgeLength = _map.HexEdgeLength;
        }

        public void OnAwake()
        {
            _moveTargets = World.GetStash<MoveTargetComponent>();
            _hexCoords = World.GetStash<HexCoordComponent>();
            _positions = World.GetStash<PositionComponent>();

            _hexPathDefinedTag = World.GetStash<HexPathDefinedTag>();
            _hexPathComponents = World.GetStash<HexPathComponent>();
            _triangularPosComponents = World.GetStash<TriangularPosComponent>();

            _awaitingComponents = World.GetStash<MovementAwaitingComponent>();

            _noPathEntitiesFilter = World.Filter
                .With<NavigationAgentComponent>()
                .With<MoveTargetComponent>()
                .Without<HexPathDefinedTag>()
                .Without<MovementAwaitingComponent>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_map.IsInitialized)
                return;

            foreach (var entity in _noPathEntitiesFilter)
            {
                var startHexCoord = _hexCoords.Get(entity).Value;
                var startPos = _positions.Get(entity).Value;

                var moveTargetComponent = _moveTargets.Get(entity);
                var endPos = moveTargetComponent.WorldPos;
                var endHexCoord = HexMath.DefineHex(endPos.xz, _hexEdgeLength);

                if (!math.all(startHexCoord == endHexCoord))
                {
                    // requests hexes data calculation, setting the awaiting component, that will be cleared when hexes are ready (look at filter)
                    if (!CheckHexDataAndRequestIfMissing(entity, startHexCoord, endHexCoord, out var startHex, out var endHex))
                        continue;
                    
                    var startTripos = _triangularPosComponents.Get(entity).Value;
                    var startZoneIndex = _map.GetPassabilityData(startTripos).ZoneIndex;
                    var endTripos = _moveTargets.Get(entity).TriangularPos;
                    var endZoneIndex = _map.GetPassabilityData(endTripos).ZoneIndex;

                    var request = new HexPathSearchRequest(
                        startHexCoord: startHexCoord, 
                        endHexCoord: endHexCoord, 
                        startTripos: startTripos,
                        endTripos: endTripos,
                        startHexZoneIndex: startZoneIndex,
                        endHexZoneIndex: endZoneIndex);

                    var searchResultData = _hexPathSearcher.TryGetHexPath(request);
                    switch (searchResultData.Result)
                    {
                        case HexPathSearchResult.PathFound:
                            {
                                _hexPathComponents.Set(entity, new(searchResultData.PathId, searchResultData.NodesCount)); 
                                break;
                            }
                        case HexPathSearchResult.CalculationNotFinished:
                            {
                                _awaitingComponents.Add(entity, new(searchResultData.ConstructionAwaitingToken));
                                break;
                            }
                        case HexPathSearchResult.OnlyIncompletePathPossible:
                            {
                                throw new System.NotImplementedException("incomplete path handling not implemented");
                            }
                        case HexPathSearchResult.PathImpossible:
                            {
                                throw new System.NotImplementedException("impossible paths handling not implemented");
                            }
                    }
                }

                _hexPathDefinedTag.Add(entity);
            }
        }

        public void Dispose() { }

        private bool CheckHexDataAndRequestIfMissing(
            Entity entity, 
            int2 startHexCoord, 
            int2 endHexCoord, 
            out INavigationHex startHex, 
            out INavigationHex endHex)
        {
            if (_hexDataAccessHandler.TryGetHexData(startHexCoord, endHexCoord, out startHex, out endHex, out AwaitingToken awaitingToken))
                return true;

            _awaitingComponents.Add(entity, new(awaitingToken));
            return false;
        }
    }
}
