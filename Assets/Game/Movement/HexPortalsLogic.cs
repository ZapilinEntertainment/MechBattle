using System.Collections.Generic;
using VContainer;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class HexPortalsLogic : HexPortalsLogicBase
    {       
        private readonly PortalDistancesCalculationRequests _distanceCalculationRequests;
        private readonly OutdatedPortalsList _outdatedPortals;
        private readonly List<(int portalId, int exitId, NavigationPortalExit exit)> _hexPointsList = new();

        [Inject]
        public HexPortalsLogic(
            HexPortalsList portals, 
            PortalDistancesCalculationRequests distanceCalculationRequests,
            OutdatedPortalsList outdatedPortals,
            IExitsLogic exitsLogic,
            PortalConnectionsList connectionsList,
            IPortalExitsList exitsList) : base(portals, connectionsList, exitsLogic, exitsList)
        {
            _distanceCalculationRequests = distanceCalculationRequests;
            _outdatedPortals = outdatedPortals;
        }

       

        public override int RegisterNewPortal(NavigationPortal portal)
        {
            var id = base.RegisterNewPortal(portal);
            _distanceCalculationRequests.Add(id);
            return id;
        }

        public override void OnPortalOutdated(int id) => _outdatedPortals.Add(id);

    }
}
