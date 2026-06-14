using Unity.Mathematics;
using Unity.Collections;
using ZE.Utils;

namespace ZE.MechBattle.Navigation
{
    public readonly struct CalculatePointDistancesLaunchData
    {
        public readonly int PortalId;
        public readonly int2 HexCoord;
        public readonly IntTriangularPos CenterPos;

        public CalculatePointDistancesLaunchData(int portalId, int2 hexCoord, IntTriangularPos centerPos)
        {
            PortalId = portalId;
            HexCoord = hexCoord;
            CenterPos = centerPos;
        }
    }

    public class CalculatePointDistancesProcessesManager : ProcessManagerBase<CalculatePointDistancesProcess, CalculatePointDistancesLaunchData, ProcessToken>
    {
        private readonly Allocator _allocator;
        private readonly INavigationMap _map;
        private readonly IHexPortalsCoordinator _portalsCoordinator;

        public CalculatePointDistancesProcessesManager(Allocator allocator, INavigationMap map, IHexPortalsCoordinator portalsCoordinator, int maxProcessesCount) : base(maxProcessesCount)
        {
            _allocator = allocator;
            _map = map;
            _portalsCoordinator = portalsCoordinator;
        }

        protected override CalculatePointDistancesProcess CreateNewProcess() => new(_allocator, _map);

        protected override ProcessToken LaunchProcess(CalculatePointDistancesLaunchData launchData, CalculatePointDistancesProcess process, int processIndex)
        {
            process.Launch(launchData);
            return new ProcessToken(processIndex, process.ProcessIteration);
        }

        protected override void HandleResults(CalculatePointDistancesProcess process)
        {
            var results = process.StopAndGetResults();
            _portalsCoordinator.ApplyPortalDistancesMap(results);
        }

       
    }
}
