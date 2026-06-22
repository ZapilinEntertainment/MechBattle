using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public interface IHexPortalsCoordinator
    {
        int RegisterNewExit(NavigationPortalExit exit, int2 hexCoord);
        void GetEdgeExits(INavigationHex hex, HexEdge edge, List<(int id, NavigationPortalExit exitData)> exitsList);
        void OnExitOutdated(int exitId);


        int RegisterNewPortal(NavigationPortal portal);
        void OnPortalOutdated(int portalId);
        void ApplyPortalDistancesMap(CalculatePointDistancesResults results);
        bool TryGetPortalConnections(int portalId, out IReadOnlyDictionary<int, float> connections);

        bool TryGetAssignedFlowMapId(int portalExitId, out int flowMapId);
        void OnFlowMapCalculated(int flowMapId, in FlowMapCalculationResults results);
        void GetHexPortalExits(int zone, int2 hexCoord, ICollection<HexExitOption> exits);
        
        IPathsList<PortalPathDestinationKey, int> GetPathsList();
    }
}
