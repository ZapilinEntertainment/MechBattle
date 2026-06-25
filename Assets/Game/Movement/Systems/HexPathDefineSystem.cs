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

        private readonly INavigationMap _map;
        private readonly float _hexEdgeLength;

        private Filter _filter;

        private Stash<MoveTargetComponent> _moveTargets;
        private Stash<HexPathDefinedTag> _hexPathDefinedTag;
        private Stash<HexPathProcessingTag> _processingTag;

        private Stash<HexCoordComponent> _hexCoords;
        private Stash<TriangularPosComponent> _triangularPosComponents;

        private Stash<HexPathSearchRequestComponent> _searchRequests;
        

        public HexPathDefineSystem(INavigationMap map)
        {
            _map = map;

            _hexEdgeLength = _map.HexEdgeLength;
        }

        public void OnAwake()
        {
            _moveTargets = World.GetStash<MoveTargetComponent>();
            _hexPathDefinedTag = World.GetStash<HexPathDefinedTag>();
            _processingTag = World.GetStash<HexPathProcessingTag>();

            _hexCoords = World.GetStash<HexCoordComponent>();
            _triangularPosComponents = World.GetStash<TriangularPosComponent>();

            _searchRequests = World.GetStash<HexPathSearchRequestComponent>();
            

            _filter = World.Filter
                .With<NavigationAgentComponent>()
                .With<MoveTargetComponent>()
                .Without<HexPathDefinedTag>()
                .Without<HexPathProcessingTag>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_map.IsInitialized)
                return;

            foreach (var entity in _filter)
            {
                var startHexCoord = _hexCoords.Get(entity).Value;
                var startTripos = _triangularPosComponents.Get(entity).Value;
                var startPosZone = _map.GetPassabilityData(startTripos).ZoneIndex;

                var moveTargetComponent = _moveTargets.Get(entity);
                var endPos = moveTargetComponent.WorldPos;
                var endHexCoord = HexMath.DefineHex(endPos.xz, _hexEdgeLength);
                var endPosZone = _map.GetPassabilityData(moveTargetComponent.TriangularPos).ZoneIndex;

                if (!math.all(startHexCoord == endHexCoord) || startPosZone != endPosZone)
                {
                    //a: delegate search work to HexPathSearchSystem
                    var startZoneIndex = _map.GetPassabilityData(startTripos).ZoneIndex;
                    var endTripos = _moveTargets.Get(entity).TriangularPos;
                    var endZoneIndex = _map.GetPassabilityData(endTripos).ZoneIndex;

                    var startPortalKey = new PortalPathDestinationKey(startHexCoord, startZoneIndex);
                    var endPortalKey = new PortalPathDestinationKey(endHexCoord, endZoneIndex);

                    _searchRequests.Set(entity, new(startPortalKey, endPortalKey));                    
                }
                else
                {
                    //b: in-hex movement, no hex path needed
                    // no requests, will be catched by HexPathReadyCheckSystem
                }

                _processingTag.Add(entity);
                _hexPathDefinedTag.Add(entity);

#if ZE_NAVIGATION_DEBUG
                if (NavigationLogger.Settings.HasFlag(NavigationLogEvents.HexPathSet))
                    UnityEngine.Debug.Log($"hex path defined for entity {entity.Id}");
#endif
            }
        }

        public void Dispose() { }
    }
}
