using Unity.Collections;
using ZE.Utils;

namespace ZE.MechBattle.Navigation
{
    public class PortalPathConstructionProcessManager : ProcessManagerBase<PortalsPathConstructionProcess, HexPathSearchRequest, PathCalculationProcessToken>
    {
        private readonly Allocator _allocator;
        private readonly INavigationMap _map;
        private readonly IPathsList<PortalPathDestinationKey, int> _pathsBuffer;
        private readonly PortalConnectionsList _portalConnectionsList;

        public PortalPathConstructionProcessManager(
            Allocator allocator,
            INavigationMap map,
            PortalConnectionsList portalConnectionsList,

            int maxProcessesCount, 
            IPathsList<PortalPathDestinationKey, int> pathsBuffer) : 
            base(maxProcessesCount)
        {
            _allocator = allocator;
            _map = map;
            _portalConnectionsList = portalConnectionsList;
            _pathsBuffer = pathsBuffer;
        }

        protected override PortalsPathConstructionProcess CreateNewProcess() =>
            new PortalsPathConstructionProcess(_allocator, _map, _pathsBuffer, _portalConnectionsList);


        // add to list inside process
        protected override void HandleResults(PortalsPathConstructionProcess process) { }

        protected override PathCalculationProcessToken LaunchProcess(HexPathSearchRequest launchData, PortalsPathConstructionProcess process, int index)
        {
            var destinations = launchData.ToDestinationsKey();
            var reservedId = _pathsBuffer.ReservePath(destinations.start, destinations.end).Id;
            process.LaunchAsync(new() { Request = launchData, ReservedPathId = reservedId });
            return new PathCalculationProcessToken(reservedId, index, process.ProcessIteration);
        }
    }
}
