using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class UpdateEdgeTrianglesNeighboursMaskCommand
    {
        public static void Execute(IUpdatableMap map, BothSideHexEdge doubleEdgeKey)
        {
            var hexPosA = new NavigationHexPosition(doubleEdgeKey.SideA.HexCoord, map);
            switch (doubleEdgeKey.SideA.Edge) 
            {
                case HexEdge.TopRight: UpdateEdgeTris<TopRightEdgeEnumerationLogic>(map, doubleEdgeKey.SideA); break;
                case HexEdge.BottomRight: UpdateEdgeTris<BottomRightEdgeEnumerationLogic>(map, doubleEdgeKey.SideA); break;
                case HexEdge.Bottom: UpdateEdgeTris<BottomEdgeEnumerationLogic>(map, doubleEdgeKey.SideA); break;
                case HexEdge.BottomLeft: UpdateEdgeTris<BottomLeftEdgeEnumerationLogic>(map, doubleEdgeKey.SideA); break;
                case HexEdge.TopLeft: UpdateEdgeTris<TopLeftEdgeEnumerationLogic>(map, doubleEdgeKey.SideA); break;
                default: UpdateEdgeTris<TopEdgeEnumerationLogic>(map, doubleEdgeKey.SideA);break;
            }
        }

        private static void UpdateEdgeTris<T>(IUpdatableMap map, HexEdgeKey edgeKey) where T : struct, IEdgeEnumerationLogic
        {
            var trianglesPerEdge = map.TrianglesPerHexEdge;
            var hexPos = new NavigationHexPosition(edgeKey.HexCoord, map);
            var borderDirPeak = edgeKey.Edge.ToNeighbourDirectionFromPeak();
            var borderDirValley = edgeKey.Edge.ToNeighbourDirectionFromValley();

            foreach (var tripos in new EdgeEnumerator<T>(trianglesPerEdge, hexPos))
            {
                UpdateCellNeighboursMask(tripos, map);

                var oppositeTripos = tripos.IsPeak ? TriangularMath.GetPeakNeighbour(tripos, borderDirPeak) : TriangularMath.GetValleyNeighbour(tripos, borderDirValley);
                UpdateCellNeighboursMask(oppositeTripos, map);
            }
        }

        private static void UpdateCellNeighboursMask(IntTriangularPos tripos, IUpdatableMap map)
        {
            var logic = new UpdateCellNeighboursMaskLogic<CellHeightData, INavigationMap>(tripos, map, map.Settings.MaxElevationDifference);
            var passability = map.GetPassabilityData(tripos);
            passability.NeighboursMask = logic.CalculateNeighboursMask();
            map.UpdateCellPassability(tripos, passability);
        }
    
    }
}
