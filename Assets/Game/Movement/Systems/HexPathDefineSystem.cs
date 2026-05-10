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

        private Filter _noPathEntitiesFilter;

        private Stash<MoveTargetComponent> _moveTargets;
        private Stash<HexCoordComponent> _hexCoords;
        private Stash<PositionComponent> _positions;
        private Stash<HexPathDefinedTag> _hexPathDefinedTag;
        private Stash<EmptyHexPathTag> _emptyHexPathTags;
        private Stash<TransitionHexPathComponent> _transitionHexPaths;
        private Stash<HexPathSelectRequestComponent> _hexPathSelectionComponents;
        private Stash<TriangularPosComponent> _triangularPosComponents;

        public HexPathDefineSystem(HexPathsLRUBuffer pathsList, INavigationMap map)
        {
            _hexPathsList = pathsList;
            _map = map;

            _hexEdgeLength = _map.HexEdgeLength;
        }

        public void OnAwake()
        {
            _moveTargets = World.GetStash<MoveTargetComponent>();
            _hexCoords = World.GetStash<HexCoordComponent>();
            _positions = World.GetStash<PositionComponent>();

            _hexPathDefinedTag = World.GetStash<HexPathDefinedTag>();
            _emptyHexPathTags = World.GetStash<EmptyHexPathTag>();
            _transitionHexPaths = World.GetStash<TransitionHexPathComponent>();
            _hexPathSelectionComponents = World.GetStash<HexPathSelectRequestComponent>();
            _triangularPosComponents = World.GetStash<TriangularPosComponent>();

            _noPathEntitiesFilter = World.Filter
                .With<NavigationAgentComponent>()
                .With<MoveTargetComponent>()
                .Without<HexPathDefinedTag>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_map.IsInitialized)
                return;

            foreach (var entity in _noPathEntitiesFilter)
            {
                var startHex = _hexCoords.Get(entity).Value;
                var startPos = _positions.Get(entity).Value;

                var moveTargetComponent = _moveTargets.Get(entity);
                var endPos = moveTargetComponent.WorldPos;
                var endHex = HexMath.DefineHex(endPos.xz, _hexEdgeLength);

                if (math.all(startHex == endHex))
                {
                    // inside same hex
                    _emptyHexPathTags.Add(entity);
                }
                else
                {
                    if (HexTransitionLogic.IsEdgeTransitionPossible(startHex, endHex, _map, out var transitionEdge))
                    {
                        // just transite into neighbour hex through edge
                        _transitionHexPaths.Add(entity, new(endHex, transitionEdge));
                    }
                    else
                    {
                        // request to make path from/to any accessible edge
                        var startTripos = _triangularPosComponents.Get(entity).Value;
                        var startHexEdgesMask = HexTransitionLogic.GetAccessibleEdgesMaskAtPosition(startTripos, _map);
                        var endHexEdgesMask = HexTransitionLogic.GetAccessibleEdgesMaskAtPosition(moveTargetComponent.TriangularPos, _map);

                        _hexPathSelectionComponents.Add(entity, new(startHex, startHexEdgesMask, endHex, endHexEdgesMask));
                    }
                }

                _hexPathDefinedTag.Add(entity);
            }
        }

        public void Dispose() { }
    }
}
