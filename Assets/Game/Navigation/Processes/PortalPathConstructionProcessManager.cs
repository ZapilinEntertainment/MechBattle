using Unity.Collections;
using ZE.Utils;

namespace ZE.MechBattle.Navigation
{
    public class PortalPathConstructionProcessManager : ProcessManagerBase<PortalsPathConstructionProcess, PortalConstructionProcessInput, PathCalculationProcessToken>
    {
        private readonly Allocator _allocator;
        private readonly INavigationMap _map;
        private readonly IHexPortalsCoordinator _portalsCoordinator;
        private readonly IPortalsLogic _portalsLogic;

        public PortalPathConstructionProcessManager(
            Allocator allocator,
            INavigationMap map,
            int maxProcessesCount,
            IHexPortalsCoordinator portalsCoordinator,
            IPortalsLogic portalsLogic) : 
            base(maxProcessesCount)
        {
            _allocator = allocator;
            _map = map;            
            _portalsCoordinator = portalsCoordinator;
            _portalsLogic = portalsLogic;
        }

        protected override PortalsPathConstructionProcess CreateNewProcess() =>
            new PortalsPathConstructionProcess(_allocator, _map, _portalsCoordinator, _portalsLogic);


        // add to list inside process
        protected override void HandleResults(PortalsPathConstructionProcess process) 
        {
            process.OnResultsApplied();
        }

        protected override PathCalculationProcessToken LaunchProcess(PortalConstructionProcessInput launchData, PortalsPathConstructionProcess process, int index)
        {
            var token = new PathCalculationProcessToken(launchData.ReservedPathId, index, process.ProcessIteration);
            LaunchProcess(launchData, process);
            return token;
        }

        private void LaunchProcess(PortalConstructionProcessInput launchData, PortalsPathConstructionProcess process)
        {
            process.LaunchAsync(launchData);
        }
    }
}
