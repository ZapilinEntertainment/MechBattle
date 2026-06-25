using Unity.Collections;
using Unity.Mathematics;
using ZE.Utils;

namespace ZE.MechBattle.Navigation
{
    public struct FlowMapProcessLaunchProtocol
    {
        public int FlowMapId;
        public int2 HexCoord;
        public NavigationPortalExit ExitData;
    }

    public class FlowMapProcessesManager : ProcessManagerBase<FlowMapCalculationProcess, FlowMapProcessLaunchProtocol, PathCalculationProcessToken>
    {
        private readonly Allocator _allocator;
        private readonly INavigationMap _map;
        private readonly IHexPortalsCoordinator _mapsCoordinator;

        public FlowMapProcessesManager(
            Allocator allocator, 
            INavigationMap map,
            IHexPortalsCoordinator mapsCoordinator,
            int maxProcessesCount) : base(maxProcessesCount)
        {
            _allocator = allocator;
            _map = map;
            _mapsCoordinator = mapsCoordinator;
        }

        protected override FlowMapCalculationProcess CreateNewProcess() => new(_allocator, _map);

        protected override PathCalculationProcessToken LaunchProcess(FlowMapProcessLaunchProtocol launchData, FlowMapCalculationProcess process, int processIndex)
        {
            //UnityEngine.Debug.Log($"start flow map calculation: {launchData.FlowMapId} at {launchData.HexCoord}");
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
