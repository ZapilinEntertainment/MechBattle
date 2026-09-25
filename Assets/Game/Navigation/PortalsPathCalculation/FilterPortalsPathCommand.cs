using System;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class FilterPortalsPathCommand
    {
        // remove excess portals in path, so no intermediate portals within hex will be presented (just entrance and exit)
        public static NativeList<int> Execute(IHexPortalsCoordinator portalsCoordinator, NativeList<int> path, int2 startHexCoord)
        {
            var length = path.Length;
            if (length == 1)
                return path;

#if UNITY_EDITOR
            if (length == 0)
            {
                UnityEngine.Debug.LogError("zero length");
                return path;
            }
#endif

            Span<int> filteredPath = stackalloc int[length];
            var filteredPathIndex = 0;
            var currentHexCoord = startHexCoord;
            var nextHexCoord = currentHexCoord;

#if UNITY_EDITOR
            var zeroPortal = portalsCoordinator.GetPortal(path[0]);
            if (math.any(zeroPortal.HexCoordA != startHexCoord) && math.any(zeroPortal.HexCoordB != startHexCoord))
                UnityEngine.Debug.LogError($"start hex coord is not correct: portal {path[0]}, hex: {startHexCoord}");
#endif


            for (var i = 0; i < length; i++)
            {
                var portalId = path[i];
                var portal = portalsCoordinator.GetPortal(portalId);

                var useExitA = math.all(portal.HexCoordA == currentHexCoord);
                var useExitB = math.all(portal.HexCoordB == currentHexCoord);
                if (useExitA | useExitB)
                {
                    // write over last portal if in same hex
                    filteredPath[filteredPathIndex] = portalId;
                    // other one is next hex
                    nextHexCoord = useExitA ? portal.HexCoordB : portal.HexCoordA;
                }
                else
                {

#if UNITY_EDITOR
                    if (filteredPathIndex + 1 >= length)
                        UnityEngine.Debug.LogError("path length mismatch");
#endif

                    filteredPath[++filteredPathIndex] = portalId;
                    // transition done, update current hexcoord
                    currentHexCoord = nextHexCoord;
                }
            }

            var newCount = filteredPathIndex + 1;
            if (newCount == length)
                return path;

            path.Length = newCount;
            for (var i = 0; i < newCount; i++)
            {
                path[i] = filteredPath[i];
            }

            return path;
        }

    }
}
