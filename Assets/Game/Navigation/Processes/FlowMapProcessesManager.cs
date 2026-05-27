using Unity.Collections;
using ZE.Utils;

namespace ZE.MechBattle.Navigation
{
    public struct FlowMapProcessLaunchProtocol
    {
        public int PortalId;
        public int FlowMapId;
        public NavigationPortalExit ExitData;
    }

    public class FlowMapProcessesManager : ProcessManagerBase<FlowMapCalculationProcess, FlowMapProcessLaunchProtocol, PathCalculationProcessToken>
    {
        private readonly Allocator _allocator;
        private readonly INavigationMap _map;
        private readonly IFlowMapsCoordinator _mapsCoordinator;

        public FlowMapProcessesManager(
            Allocator allocator, 
            INavigationMap map,
            IFlowMapsCoordinator mapsCoordinator,
            int maxProcessesCount) : base(maxProcessesCount)
        {
            _allocator = allocator;
            _map = map;
            _mapsCoordinator = mapsCoordinator;
        }

        protected override FlowMapCalculationProcess CreateNewProcess() => new(_allocator, _map);

        protected override PathCalculationProcessToken LaunchProcess(FlowMapProcessLaunchProtocol launchData, FlowMapCalculationProcess process, int processIndex)
        {
            process.Launch(launchData);
            return new PathCalculationProcessToken(launchData.FlowMapId, processIndex, process.ProcessIteration);
        }

        protected override void HandleResults(FlowMapCalculationProcess process)
        {
            var results = process.StopAndGetResults();
            _mapsCoordinator.OnFlowMapCalculated(process.ActiveProtocol.FlowMapId, results);
        }
    }
}
