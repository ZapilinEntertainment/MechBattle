using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Burst;

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
            var oppositeEdge = transitionEdge.ToOpposite();
            return IsEdgeTransitionPossible(hexCoordA, hexCoordB, transitionEdge, oppositeEdge, map);
        }

        public static bool IsEdgeTransitionPossible(HexPathNodeKey node, INavigationMap map)
        {
            var oppositeNode = node.ToOpposite();
            return IsEdgeTransitionPossible(node.HexCoord, oppositeNode.HexCoord, node.Edge, oppositeNode.Edge, map);
        }

        public static bool IsEdgeTransitionPossible(
            int2 hexCoordA, 
            int2 hexCoordB, 
            HexEdge transitionEdgeA, 
            HexEdge transitionEdgeB, 
            INavigationMap map) =>
            map.TryGetHex(hexCoordA, out var hexA) && map.TryGetHex(hexCoordB, out var hexB)
               ? hexA.EdgesPassability.IsEdgePresented(transitionEdgeA) && hexB.EdgesPassability.IsEdgePresented(transitionEdgeB)
               : map.DefaultPassability;


        public static HexEdgesMask GetAccessibleEdgesMaskAtPosition(IntTriangularPos pos, INavigationMap map)
        {
            var flowData = map.GetFlowData(pos);
            return flowData.GetCombinedEdgeAccessMask();
        }
    }
}
