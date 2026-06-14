using VContainer;

namespace ZE.MechBattle.Navigation
{
    // local realization version
    public class HexExitsLogic : HexExitsLogicBase
    {
        private readonly OutdatedExitsList _outdatedExitsList;
        private readonly FlowMapAssignmentList _flowMapAssignmentList;


        [Inject]
        public HexExitsLogic(
            PortalExitsList exitsList, 
            OutdatedExitsList outdatedExitsList, 
            IUpdatableMap map, 
            IHexPortalsList portalsList,
            FlowMapAssignmentList flowMapAssignmentList) : 
            base(exitsList, map, portalsList) 
        {
            _outdatedExitsList = outdatedExitsList;
            _flowMapAssignmentList = flowMapAssignmentList;
        }

        public override void OnExitOutdated(int exitId) => _outdatedExitsList.Add(exitId);

        public override void RemoveExit(int exitId)
        {
            base.RemoveExit(exitId);
            _flowMapAssignmentList.RemoveBond(exitId);
        }
    }
}
