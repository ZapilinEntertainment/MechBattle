using Unity.Collections;
using ZE.Utils;

namespace ZE.MechBattle.Navigation
{
    public class PortalPathConstructionProcessManager : ProcessManagerBase<PortalsPathConstructionProcess, PortalConstructionProcessInput, PathCalculationProcessToken>
    {
        private readonly Allocator _allocator;
        private readonly INavigationMap _map;
        private readonly IHexPortalsCoordinator _portalsCoordinator;
        private readonly IPathsList<PortalPathDestinationKey, int> _pathsBuffer;

        public PortalPathConstructionProcessManager(
            Allocator allocator,
            INavigationMap map,
            int maxProcessesCount,
            IHexPortalsCoordinator portalsCoordinator) : 
            base(maxProcessesCount)
        {
            _allocator = allocator;
            _map = map;            
            _portalsCoordinator = portalsCoordinator;
            _pathsBuffer = _portalsCoordinator.GetPathsList();
        }

        protected override PortalsPathConstructionProcess CreateNewProcess() =>
            new PortalsPathConstructionProcess(_allocator, _map, _portalsCoordinator);


        // add to list inside process
        protected override void HandleResults(PortalsPathConstructionProcess process) { }

        protected override PathCalculationProcessToken LaunchProcess(PortalConstructionProcessInput launchData, PortalsPathConstructionProcess process, int index)
        {
            process.LaunchAsync(launchData);
            return new PathCalculationProcessToken(launchData.ReservedPathId, index, process.ProcessIteration);
        }
    }
}
