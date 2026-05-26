using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class HexPortalsList : Dictionary<int, NavigationPortal>
    {
        public bool TryGetPortalExit(int2 hexCoord, int portalId, out NavigationPortalExit exit, out bool isExitA)
        {
            if (!TryGetValue(portalId, out var navigationPortal))
            {
                exit = default;
                isExitA = false;
                return false;
            }

            exit = navigationPortal.GetExit(hexCoord);
            isExitA = math.all(hexCoord == navigationPortal.ExitA.HexCoord);

            return math.all(hexCoord == exit.HexCoord);
        }
    }
}
