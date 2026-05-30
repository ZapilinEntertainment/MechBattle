using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Collections;
using ZE.Utils;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FlowMapCalculationSystem : PathCalculationSystemBase<PortalExitFlowMap>
    {
        protected override int MAX_CACHED_STATUSES_COUNT => 64;
        protected override Filter Filter => _filter;
        protected override IProcessManager<PathCalculationProcessToken> ProcessManager => _processesManager;
        protected override IEntityPathValidator<PortalExitFlowMap> PathValidator => _validator;

        private Filter _filter;
        private EntityPathValidator<PortalExitFlowMap, FlowTrianglePathComponent, ClearTrianglePathTag> _validator;
        private Stash<FlowMapCalculationTag> _calculationTag;

        private readonly FlowMapProcessesManager _processesManager;
        private readonly HexPortalsCoordinator _flowMapsCoordinator;

        private const int MAX_PROCESSES = 4;

        public FlowMapCalculationSystem(INavigationMap map, HexPortalsCoordinator flowMapsCoordinator)
        {
            _flowMapsCoordinator = flowMapsCoordinator;
            _processesManager = new (Allocator.Persistent, map, _flowMapsCoordinator, MAX_PROCESSES);
            
        }

        public override void Dispose()
        {
            _processesManager.Dispose();
        }

        public override void OnAwake()
        {
            _filter = World.Filter.With<FlowMapCalculationTag>().Build();

            _validator = new(World, PathStatusesLRU, _flowMapsCoordinator.MapsList);
        }

        protected override void OnPathCompleted(Entity entity, PortalExitFlowMap path)
        {
            _calculationTag.Remove(entity);
        }

        protected override bool TryStartCalculation(Entity entity, PortalExitFlowMap flowMap, out PathCalculationProcessToken token)
        {
            if (!_flowMapsCoordinator.TryGetFlowMapPortalExit(flowMap.Id, out var exitData))
            {
                token = default;
                return false;
            }

            var protocol = new FlowMapProcessLaunchProtocol()
            {
                FlowMapId = flowMap.Id,
                HexCoord = flowMap.HexCoord,
                ExitData = exitData
            };
            token = _processesManager.TryLaunchProcess(protocol);
            return token.IsValid;
        }

        
    }
}