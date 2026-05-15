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


        public static CellHexAccessData GetAccessibleEdgesMaskAtPosition(IntTriangularPos pos, INavigationMap map)
        {
            var flowData = map.GetFlowData(pos);
            return new(flowData.GetCombinedEdgeAccessMask(), new CombinedExitDistances(flowData));
        }

        public static float6 GetDirectionCostCoefficients(int2 startHexCoord, int2 endHexCoord)
        {
            var cf = new float6();
            var dir = math.normalize(endHexCoord - startHexCoord);
            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                cf[edge] = 0.2f * math.dot(dir, math.normalize(edge.ToHexOffsetVector()));
            }
            return cf;
        }
    }
}
