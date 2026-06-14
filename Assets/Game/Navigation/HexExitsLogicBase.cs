using System;
using System.Collections.Generic;

namespace ZE.MechBattle.Navigation
{
    public interface IExitsLogic 
    {
        void OnExitOutdated(int exitId);
        void ActualizeExitsList(
            List<(int id, NavigationPortalExit exitData)> actualData,
            List<NavigationPortalExit> newData,
            IUpdatableNavigationHex hex);

        bool TryGetExitDataWithValidation(int exitId, out NavigationPortalExit exitData);
        void RemoveExit(int exitId);
        int RegisterNewExit(NavigationPortalExit exit, IUpdatableNavigationHex hex);
    }

    public class HexExitsLogicBase : IExitsLogic
    {
        private readonly PortalExitsList _exitsList;
        private readonly IHexPortalsList _portalsList;
        private readonly IUpdatableMap _map;

        public HexExitsLogicBase(PortalExitsList exitsList, IUpdatableMap map, IHexPortalsList portalsList)
        {
            _exitsList = exitsList;
            _map = map;
            _portalsList = portalsList;
        }

        public bool TryGetExitDataWithValidation(int exitId, out NavigationPortalExit exitData)
        {
            if (_exitsList.TryGetValue(exitId, out exitData))
                return true;

            OnExitOutdated(exitId);
            return false;
        }

        public virtual void OnExitOutdated(int exitId) { }

        public void ActualizeExitsList(
            List<(int id, NavigationPortalExit exitData)> actualData,
            List<NavigationPortalExit> newData,
            IUpdatableNavigationHex hex)
        {
            const int INVALID_EXIT_ID = -1;

            var length = newData.Count;
            Span<int> ids = stackalloc int[length];
            for (var i = 0; i < length; i++)
            {
                ids[i] = INVALID_EXIT_ID;
            }

            // get rid of outdated exits
            foreach (var exitCD in actualData)
            {
                // cd is combined data
                var matchFound = false;
                for (var i = 0; i < length; i++)
                {
                    var exitData = newData[i];
                    if (exitData == exitCD.exitData)
                    {
                        ids[i] = exitCD.id;
                        matchFound = true;
                        break;
                    }
                }

                if (!matchFound)
                {
                    var outdatedId = exitCD.id;
                    OnExitOutdated(outdatedId);
                    hex.Exits.Remove(outdatedId);
                }
            }

            // register new exits (without ids)
            for (var i = 0; i < length; i++)
            {
                var exitId = ids[i];
                if (exitId == INVALID_EXIT_ID)
                {
                    var newId = RegisterNewExit(newData[i], hex);
                    ids[i] = newId;
                }
            }
        }

        public int RegisterNewExit(NavigationPortalExit exit, IUpdatableNavigationHex hex)
        {
            var id = _exitsList.RegisterExit(exit);
            hex.Exits.Add(id);
            return id;
        }

        public virtual void RemoveExit(int exitId)
        {
            if (!_exitsList.TryGetValue(exitId, out var exit))
                return;

            _exitsList.Remove(exitId);
            var hexCoord = TriangularMath.TriangularToHex(exit.StartTriangle, _map.TriangleHeight, _map.HexEdgeLength);
            _map.GetOrCreateUpdatableHex(hexCoord).Exits.Remove(exitId);

            // unexisting portal exit data will be actualized by another systems
        }
    }
}
