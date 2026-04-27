using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation
{
    public static class UpdateHexEdgesPassabilityCommand
    {
        public static void Execute(IUpdatableMap map, DefineTransitionTrianglesJobCollection collection)
        {
            var mapSettings = map.Settings;
            var hexRadius = mapSettings.TrianglesPerHexEdge;
            var hexEdge = mapSettings.HexEdgeSize;

            var job = PrepareTransitionTrianglesJob(map, collection);
            job.RunByRef(collection.CalculatingNodes.Length);
            
            UpdateBorderTriangles(map, collection);

            foreach (var hexCoord in map.HexCoords)
                UpdateHexEdgePassabilities(map, hexCoord);

            map.UpdateVersion();
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

        private static void UpdateBorderTriangles(IUpdatableMap map, DefineTransitionTrianglesJobCollection collection)
        {
            foreach (var posData in collection.Results)
            {
                //UnityEngine.Debug.Log(pos);
                UpdateTrianglePassability(map, posData.xyz);
                var oppositePos = TriangularMath.GetNeighbourByDirection(posData.xyz, posData.w);
                UpdateTrianglePassability(map, oppositePos);
            }
        }

        private static void UpdateTrianglePassability(IUpdatableMap map, IntTriangularPos pos)
        {
            var passability = map.GetPassabilityData(pos);
            var neighboursMask = 0;
            for (var i = 0; i < NavigationConstants.TRIANGLE_DIRECTIONS_COUNT; i++)
            {
                var neighbourPos = TriangularMath.GetNeighbourByDirection(pos, i);
                if (TrianglesTransitionLogic.IsCloseTransitionPossible(map, pos, neighbourPos))
                    neighboursMask |= (1 << i);
            }
            TrianglesTransitionLogic.CheckMaskForJumpNeighbours(neighboursMask, pos.IsPeak);
            map.UpdateCellPassability(pos, new(passability.IsPassable, neighboursMask, passability.EntranceCost)); 
        }

        private static void UpdateHexEdgePassabilities(IUpdatableMap map, int2 hexCoord)
        {
            var mask = 0;
            var hexPos = new NavigationHexPosition(hexCoord, map);

            if (TryFindPassage<TopEdgeEnumerationLogic>(new(map.TrianglesPerHexEdge, hexPos), map, HexEdge.Top)) 
                mask |= (1 << (int)HexEdge.Top);

            if (TryFindPassage<TopRightEdgeEnumerationLogic>(new(map.TrianglesPerHexEdge, hexPos), map, HexEdge.TopRight))
                mask |= (1 << (int)HexEdge.TopRight);

            if (TryFindPassage<BottomRightEdgeEnumerationLogic>(new(map.TrianglesPerHexEdge, hexPos), map, HexEdge.BottomRight))
                mask |= (1 << (int)HexEdge.BottomRight);

            if (TryFindPassage<BottomEdgeEnumerationLogic>(new(map.TrianglesPerHexEdge, hexPos), map, HexEdge.Bottom))
                mask |= (1 << (int)HexEdge.Bottom);

            if (TryFindPassage<BottomLeftEdgeEnumerationLogic>(new(map.TrianglesPerHexEdge, hexPos), map, HexEdge.BottomLeft))
                mask |= (1 << (int)HexEdge.BottomLeft);

            if (TryFindPassage<TopLeftEdgeEnumerationLogic>(new(map.TrianglesPerHexEdge, hexPos), map, HexEdge.TopLeft))
                mask |= (1 << (int)HexEdge.TopLeft);

            var hex = map.GetHex(hexCoord);
            hex.UpdateEdgesPassability(new(mask));
            hex.UpdateVersion();
        }

        private static bool TryFindPassage<T>(EdgeEnumerator<T> enumerator, INavigationMap map, HexEdge edge) where T : unmanaged, IEdgeEnumerationLogic
        {
            var peakDir = (int)edge.ToNeighbourDirectionFromPeak();
            var valleyDir = (int)edge.ToNeighbourDirectionFromValley();
            foreach (var pos in enumerator)
            {
                var neighbourMask = map.GetPassabilityData(pos).NeighboursMask;
                var directionMask = 1 << (pos.IsPeak ? peakDir : valleyDir);
                if ((neighbourMask & directionMask) != 0)
                {
                    return true;
                }     
            }

            return false;
        }
    }
}
