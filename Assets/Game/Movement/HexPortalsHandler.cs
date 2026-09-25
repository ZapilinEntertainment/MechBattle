using VContainer;

namespace ZE.MechBattle.Navigation
{
    public class HexPortalsHandler : HexPortalsHandlerBase
    {       
        private readonly PortalDistancesCalculationRequests _distanceCalculationRequests;
        private readonly OutdatedPortalsList _outdatedPortals;

        [Inject]
        public HexPortalsHandler(
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
