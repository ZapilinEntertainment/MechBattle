using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class FormPortalsCommand
    {
        public static void Execute(INavigationMap map, int2 hexCoord, HexEdge edge, List<NavigationPortal> portalsList)
        {
            var innerPortals = new List<NavigationPortalExit>();
            CalculateHexExitsCommand.Execute(map, hexCoord, edge, innerPortals);
            var oppositeHex = new HexPathNodeKey(hexCoord, edge).ToOpposite();

            var outerPortals = new List<NavigationPortalExit>();
            CalculateHexExitsCommand.Execute(map, oppositeHex.HexCoord, oppositeHex.Edge, outerPortals);

            if (innerPortals.Count == 0 || outerPortals.Count == 0)
                return;

            var length = map.TrianglesPerHexEdge;
            Span<int> innerZoneIndices = stackalloc int[length];
            Span<int> outerZoneIndices = stackalloc int[length];

            FulfillZoneIndices(edge,  innerZoneIndices, innerPortals);
            FulfillZoneIndices(oppositeHex.Edge, outerZoneIndices, outerPortals);


        }

        private static void FulfillZoneIndices(HexEdge edge, Span<int> zoneIndices, IReadOnlyList<NavigationPortalExit> exitList)
        {
            switch (edge)
            {
                case HexEdge.TopRight: FulfillZoneIndices<TopRightEdgeEnumerationLogic>(zoneIndices, exitList); break;
                case HexEdge.BottomRight: FulfillZoneIndices<BottomRightEdgeEnumerationLogic>(zoneIndices, exitList); break;
                case HexEdge.Bottom: FulfillZoneIndices<BottomEdgeEnumerationLogic>(zoneIndices, exitList); break;
                case HexEdge.BottomLeft: FulfillZoneIndices<BottomLeftEdgeEnumerationLogic>(zoneIndices, exitList); break;
                case HexEdge.TopLeft: FulfillZoneIndices<TopLeftEdgeEnumerationLogic>(zoneIndices, exitList); break;
                default: FulfillZoneIndices<TopEdgeEnumerationLogic>(zoneIndices, exitList); break;
            }
        }

        private static void FulfillZoneIndices<T>(Span<int> zoneIndices, IReadOnlyList<NavigationPortalExit> exitList) 
            where T : struct, IEdgeEnumerationLogic
        {

        }
    
    }
}
