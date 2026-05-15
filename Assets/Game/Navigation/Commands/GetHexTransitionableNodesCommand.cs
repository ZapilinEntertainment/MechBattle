using System.Collections.Generic;

namespace ZE.MechBattle.Navigation
{
    public class GetHexTransitionableNodesCommand
    {
        /// <summary>
        /// return hex nodes that have hexes on both side ((0,0):Top - (0,1):Bottom)
        /// check edge passability if needed
        /// </summary>
        public static HexTransitionableNodes Execute(INavigationMap map, bool checkEdgesPassability)
        {
            var nodesList = new HashSet<HexPathNodeKey>();
            foreach (var hexCoord in map.HexCoords)
            {
                for (var edgeIndex = 0; edgeIndex < 6; edgeIndex++)
                {
                    var edge = (HexEdge)edgeIndex;
                    var node = new HexPathNodeKey(hexCoord, edge);
                    var oppositeNode = node.ToOpposite();
                    if (map.ContainsHex(oppositeNode.HexCoord)
                        && !nodesList.Contains(oppositeNode)
                        && (!checkEdgesPassability || HexTransitionLogic.IsEdgeTransitionPossible(node.HexCoord, oppositeNode.HexCoord, edge, oppositeNode.Edge, map)))
                    {
                        nodesList.Add(node);
                    }
                }
            }
            return new(nodesList);
        }
    }
}
