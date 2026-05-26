using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FlowPathSearchSystem : ISystem 
    {
        public World World { get; set;}
        private readonly HexPortalsList _portalsList;
        private readonly FlowMapsCoordinator _flowMapsCoordinator;
        private Filter _filter;
        private Stash<FlowMapSearchRequestComponent> _searchRequests;
        private Stash<HexCoordComponent> _hexCoords;
        private Stash<ClearTrianglePathTag> _clearTags;

        [Inject]
        public FlowPathSearchSystem(HexPortalsList portalsList, FlowMapsCoordinator flowMapsCoordinator)
        {
            _portalsList = portalsList;
            _flowMapsCoordinator = flowMapsCoordinator;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<FlowMapSearchRequestComponent>().Build();
            _searchRequests = World.GetStash<FlowMapSearchRequestComponent>();
            _hexCoords = World.GetStash<HexCoordComponent>();
            _clearTags = World.GetStash<ClearTrianglePathTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var portalId = _searchRequests.Get(entity).PortalId;
                var hexCoord = _hexCoords.Get(entity).Value;
                if (!_portalsList.TryGetPortalExit(hexCoord, portalId, out var portalExit, out var isExitA))
                {
                    _clearTags.Set(entity);
                    continue;
                }

                var portalExitKey = new PortalExitFlowMapKey(portalId, isExitA);
                if (!_flowMapAssignmentList.TryGetExitFlowMap(portalExitKey, out var flowMapId))
                    flowMapId = 
            }
        }

        public void Dispose()
        {

        }
    }
}