using System.Collections.Generic;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Navigation.Tests
{
    public class TestCoordinator : IHexPortalsCoordinator
    {
        public readonly IExitsLogic ExitsLogic;
        private readonly PortalExitsList _exits;
        private readonly HexPortalsList _portalsList;
        private readonly PortalConnectionsList _connectionsList;
        private readonly IUpdatableMap _map;

        private readonly IPortalsLogic _portalsLogic;


        public TestCoordinator(PortalExitsList exits, HexPortalsList portalsList, IUpdatableMap map, PortalConnectionsList connectionsList)
        {
            _exits = exits;
            _portalsList = portalsList;
            _map = map;
            _connectionsList = connectionsList;

            ExitsLogic = new HexExitsLogicBase(_exits, _map, _portalsList);
            _portalsLogic = new HexPortalsLogicBase(_portalsList, _connectionsList, ExitsLogic, _exits);
        }

        public void ApplyPortalDistancesMap(CalculatePointDistancesResults results) => _portalsLogic.ApplyPortalDistancesMap(results);

        public void GetEdgeExits(INavigationHex hex, HexEdge edge, List<(int id, NavigationPortalExit exitData)> exitsList)
        {
            foreach (var exitId in hex.PortalExitIds)
            {
                if (_exits.TryGetValue(exitId, out var exitData) && exitData.Edge == edge)
                {
                    exitsList.Add((exitId, exitData));
                }
            }
        }

        public void GetHexPortalExits(int2 hexCoord, ICollection<HexExitOption> exits)
        {
            throw new System.NotImplementedException();
        }

        public IPathsList<PortalPathDestinationKey, int> GetPathsList()
        {
            throw new System.NotImplementedException();
        }

        public void OnFlowMapCalculated(int flowMapId, in FlowMapCalculationResults results)
        {
            throw new System.NotImplementedException();
        }

        public void OnPortalOutdated(int portalId) => _portalsList.Remove(portalId);

        public int RegisterNewExit(NavigationPortalExit exit, int2 hexCoord) => ExitsLogic.RegisterNewExit(exit, _map.GetOrCreateUpdatableHex(hexCoord));
        public void OnExitOutdated(int exitId) => ExitsLogic.RemoveExit(exitId);


        public int RegisterNewPortal(NavigationPortal portal) => _portalsList.RegisterNewPortal(portal);

        public bool TryGetAssignedFlowMapId(int portalExitId, out int flowMapId)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetPortalConnections(int portalId, out IReadOnlyDictionary<int, float> connections)
        {
            throw new System.NotImplementedException();
        }
    }
}
