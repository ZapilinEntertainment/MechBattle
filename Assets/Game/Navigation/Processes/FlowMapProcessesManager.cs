using Unity.Collections;
using ZE.Utils;

namespace ZE.MechBattle.Navigation
{
    public struct FlowMapProcessLaunchProtocol
    {
        public int PortalId;
        public NavigationPortalExit ExitData;
        public bool IsExitA;
    }

    public class FlowMapProcessesManager : ProcessManagerBase<FlowMapCalculationProcess, FlowMapProcessLaunchProtocol, PathCalculationProcessToken>
    {
        private readonly Allocator _allocator;
        private readonly INavigationMap _map;
        private readonly FlowMapsCoordinator _mapsCoordinator;

        public FlowMapProcessesManager(
            Allocator allocator, 
            INavigationMap map,
            FlowMapsCoordinator mapsCoordinator,
            int maxProcessesCount) : base(maxProcessesCount)
        {
            _allocator = allocator;
            _map = map;
            _mapsCoordinator = mapsCoordinator;
        }

        protected override FlowMapCalculationProcess CreateNewProcess() => new(_allocator, _map);

        protected override PathCalculationProcessToken LaunchProcess(FlowMapProcessLaunchProtocol launchData, FlowMapCalculationProcess process, int processIndex)
        {
            var id = _mapsCoordinator.ReserveId(new(launchData.PortalId, launchData.IsExitA));
            process.Launch(launchData);
            return new PathCalculationProcessToken(id, processIndex, process.ProcessIteration);
        }

        protected override void HandleResults(FlowMapCalculationProcess process)
        {
            var results = process.StopAndGetResults();
            var flowMap = _mapsCoordinator.CreateEmptyFlowMap(process.ActiveProtocol.ExitData.HexCoord);
            for (var i = 0; i < results.Length; i++)
            {
                flowMap[i] = results[i];
            }


        }

        
    }
}
