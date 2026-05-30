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
        private readonly HexPortalsCoordinator _flowMapsCoordinator;
        private Filter _filter;

        private Stash<FlowMapSearchRequestComponent> _searchRequests;
        private Stash<HexCoordComponent> _hexCoords;
        private Stash<ClearTrianglePathTag> _clearTags;
        private Stash<FlowTrianglePathComponent> _flowPaths;
        private Stash<FlowMapCalculationTag> _flowMapCalculationTags;

        [Inject]
        public FlowPathSearchSystem(HexPortalsCoordinator flowMapsCoordinator)
        {
            _flowMapsCoordinator = flowMapsCoordinator;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<FlowMapSearchRequestComponent>().Build();

            _searchRequests = World.GetStash<FlowMapSearchRequestComponent>();
            _hexCoords = World.GetStash<HexCoordComponent>();
            _clearTags = World.GetStash<ClearTrianglePathTag>();
            _flowPaths = World.GetStash<FlowTrianglePathComponent>();
            _flowMapCalculationTags = World.GetStash<FlowMapCalculationTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                _searchRequests.Remove(entity);

                var portalId = _searchRequests.Get(entity).PortalId;
                var hexCoord = _hexCoords.Get(entity).Value;
                if (!_flowMapsCoordinator.TryGetPortalExitId(hexCoord, portalId, out var exitId)
                    || !_flowMapsCoordinator.TryGetExitDataWithValidation(exitId, out var exitData))
                {
                    _clearTags.Set(entity);
                    continue;
                }

                if (!_flowMapsCoordinator.TryGetAssignedFlowMapId(exitId, out var flowMapId))
                {
                    var reservedFlowMap = _flowMapsCoordinator.ReserveFlowMap(exitId, exitData, hexCoord);
                    flowMapId = reservedFlowMap.Id;
                    _flowMapCalculationTags.Add(entity);
                }
                _flowPaths.Set(entity, new(flowMapId, hexCoord));
            }
        }

        public void Dispose()
        {

        }
    }
}