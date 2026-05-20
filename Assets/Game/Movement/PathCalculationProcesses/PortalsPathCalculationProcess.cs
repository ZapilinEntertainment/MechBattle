using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class PortalsPathCalculationProcess : PathCalculationProcess<PortalSearchData>
    {
        private readonly struct PortalOption
        {
            public readonly int PortalId;
            public readonly float PortalDistance;

            public PortalOption(int portalId, float portalDistance)
            {
                PortalId = portalId;
                PortalDistance = portalDistance;
            }
        } 

        private readonly INavigationMap _map;
        private readonly CalculateHexCellDistancesProcess _distancesProcess;
        private AwaitingToken _currentAwaitToken;

        public PortalsPathCalculationProcess(Allocator allocator, INavigationMap map) 
        { 
            _map = map;
            _distancesProcess = new(allocator, map);
        }

        protected override JobHandle LaunchJob(PortalSearchData start, PortalSearchData end)
        {
            // prepare start portal map - job 1
            // prepare end portal map - job 2
            // construct simple path - job 3

            var startDistancesHandle = _distancesProcess.Schedule(start.HexCoord, start.Tripos);
            var endDistancesHandle = _distancesProcess.Schedule(end.HexCoord, end.Tripos, startDistancesHandle);

            // prepare ordered portals list
            // build portal paths
            // select shortest one
            // check it for accuracy
            // if it has mistakes, get next one
        }


        protected override PathCalculationResult<PortalSearchData> FormResults()
        {
            
        }

             

        protected override void DisposeResources()
        {
            _distancesProcess.Dispose();            
        }
    }
}
