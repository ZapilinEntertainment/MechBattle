using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation
{
    public static class UpdateHexEdgesPassabilityCommand
    {
        public static void Execute(INavigationMap map, DefineTransitionTrianglesJobCollection collection)
        {
            var mapSettings = map.Settings;
            var hexRadius = mapSettings.TrianglesPerHexEdge;
            var hexEdge = mapSettings.HexEdgeSize;

            var job = PrepareTransitionTrianglesJob(map, collection);
            job.RunByRef(collection.CalculatingNodes.Length);

            //for (var i = 0; i < collection.CalculatingNodes.Length; i++)
            //{
            //    var hexCoord = collection.CalculatingNodes[i].HexCoord;
            //    var flowMap = map.GetFlowMap(hexCoord);
            //    for (var a = 0; a < collection.TrianglesPerNode; a++)
            //    {
            //        var passa
            //    }
            //}
            //foreach (var posData in collection.Results)
            //{                
            //    var cellPassability = map.GetPassabilityData(posData.xyz);
            //    var cellHeight = map.GetCellHeights(posData.xyz);

            //    var neighbourPos = TriangularMath.GetNeighbourByDirection(posData.xyz, posData.w);
            //    var neighbourPassability = map.GetPassabilityData(neighbourPos);
            //    var neighbourHeight = map.GetCellHeights(neighbourPos);


            //}
        }

        private static EdgePositionsDefineJob PrepareTransitionTrianglesJob(INavigationMap map, DefineTransitionTrianglesJobCollection collection)
        {
            //1. get mirrored nodes
            var nodesList = new HashSet<HexPathNodeKey>();
            foreach (var hexCoord in map.HexCoords)
            {
                for (var edgeIndex = 0; edgeIndex < 6; edgeIndex++)
                {
                    var edge = (HexEdge)edgeIndex;
                    var node = new HexPathNodeKey(hexCoord, edge);
                    var oppositeNode = node.ToOpposite();
                    if (map.ContainsHex(oppositeNode.HexCoord) && !nodesList.Contains(oppositeNode))
                        nodesList.Add(node);
                }
            }

            // 2.form triangles list for each edge (both sides of node)

            collection.Update(nodesList, map.TrianglesPerHexEdge);
            return new EdgePositionsDefineJob()
            {
                CalculatingNodes = collection.CalculatingNodes,
                HexEdgeSize = map.HexEdgeSize,
                HexRadius = map.TrianglesPerHexEdge,
                Results = collection.Results,
                TrianglesPerNode = collection.TrianglesPerNode
            };
        }

    }
}
