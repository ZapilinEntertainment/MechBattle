using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class HexTransitionLogic
    {
        public static bool IsEdgeTransitionPossible(int2 hexCoordA, int2 hexCoordB, INavigationMap map, out HexEdge transitionEdge)
        {
            if (!HexMath.AreNeighbours(hexCoordA, hexCoordB))
            {
                transitionEdge = default;
                return false;
            }

            var dir = math.sign(hexCoordB - hexCoordA);
            transitionEdge = HexMath.HexOffsetVectorToEdge(dir);
            return map.TryGetHex(hexCoordA, out var hexA) 
                ? hexA.EdgesPassability.IsEdgePresented(transitionEdge)
                : map.DefaultPassability;
        }

        public static HexEdgesMask GetAccessibleEdgesMaskAtPosition(IntTriangularPos pos, INavigationMap map)
        {
            var flowData = map.GetFlowData(pos);
            return flowData.GetCombinedEdgeAccessMask();
        }
    
    }
}
