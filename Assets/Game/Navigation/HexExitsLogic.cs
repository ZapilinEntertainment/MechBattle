using System;
using System.Collections.Generic;

namespace ZE.MechBattle.Navigation
{
    public struct HexExitsLogic
    {
        private readonly IHexPortalsCoordinator _portalsCoordinator;
        private readonly PortalExitsList _exitsList;

        public HexExitsLogic(IHexPortalsCoordinator portalsCoordinator, PortalExitsList exitsList)
        {
            _portalsCoordinator = portalsCoordinator;
            _exitsList = exitsList;
        }

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
                    _portalsCoordinator.OnExitOutdated(outdatedId);
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
    }
}
