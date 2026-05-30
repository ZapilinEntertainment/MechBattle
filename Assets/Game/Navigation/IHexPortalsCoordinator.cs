using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct HexExitOption
    {
        public readonly int ExitId;
        public readonly int PortalId;
        public readonly NavigationPortalExit ExitData;

        public HexExitOption(int portalId, int exitId, NavigationPortalExit exitData)
        {
            PortalId = portalId;
            ExitId = exitId;
            ExitData = exitData;
        }
    }

    public interface IHexPortalsCoordinator
    {
        bool TryGetAssignedFlowMapId(int portalExitId, out int flowMapId);
        void OnFlowMapCalculated(int flowMapId, in FlowMapCalculationResults results);
        void GetHexPortalExits(int2 hexCoord, List<HexExitOption> exits);
        bool TryGetPortalConnections(int portalId, out IReadOnlyDictionary<int, float> connections);
        IPathsList<PortalPathDestinationKey, int> GetPathsList();
    }
}
