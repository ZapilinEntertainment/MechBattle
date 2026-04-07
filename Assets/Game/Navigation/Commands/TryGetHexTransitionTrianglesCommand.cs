using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class TryGetHexTransitionTrianglesCommand
    {
        public readonly struct Result
        {
            public readonly bool IsSucceed;
            public readonly IntTriangularPos Start;
            public readonly IntTriangularPos End;

            public Result(IntTriangularPos start, IntTriangularPos end)
            {
                Start = start;
                End = end;
                IsSucceed = true;
            }

            public static Result Failed => new();
        }

        private struct EdgeCellData
        {
            public bool IsPassable;
            public bool IsExit;
        }

        /// <summary>
        /// trying to find connected triangles of two neighboured hexes edge
        /// </summary>
        /// <param name="farEndPos"?>
        /// point, to which a closest transition exit point will found
        /// </param>
        /// <returns>
        /// returns closest triangle pos
        /// </returns>
        /// 
        public static Result Execute(
            INavigationMap map,
            HexPathNodeKey startNode,
            IntTriangularPos startPos,
            IntTriangularPos farEndPos)
        {

            var exitNode = startNode.ToOpposite();
            var edgeTrianglesCount = TriangularMath.GetTwoRowEdgeTrianglesCount(map.TrianglesPerHexEdge);
            var cellsDictionary = new Dictionary<IntTriangularPos, EdgeCellData>(edgeTrianglesCount * 2);            
            
            var startHexPos = new NavigationHexPosition(startNode, map);
            FulfillEdgesList(startNode, cellsDictionary, map, false);

            var endHexPos = new NavigationHexPosition(exitNode, map);
            FulfillEdgesList(exitNode, cellsDictionary, map, true);

            var minDistance = int.MaxValue;
            var closestThisHexPos = startPos;
            var closestOtherHexPos = closestThisHexPos;
            var transitionsFound = 0;

            // mask contains info about which triangle neighbours can be exited into
            // (ex. if triangle is peak, and transition is (top hex 1 -> bottom hex2 ),
            // move can be done into 3 its neighbours:  vertex up left, vertex up, vertex up right,
            // and other ones are still in its own hex or on other neihbour edge)

            var peakExitsMask = startNode.Edge.GetPeakTriangleEdgeNeighboursMask();
            var valleyExitsMask = startNode.Edge.GetValleyTriangleEdgeNeighboursMask();

            foreach (var cellKvp in cellsDictionary)
            {
                var pos = cellKvp.Key;
                var data = cellKvp.Value;
                if (data.IsPassable | data.IsExit)
                    continue;
                
                var isPeak = pos.IsPeak;
                var minNextPosDistance = int.MaxValue;
                var nextNeighbourPos = pos;

                for (var i = 0; i < 12; i++)
                {
                    var mask = isPeak ? peakExitsMask : valleyExitsMask;
                    if (!mask.IsSet(i))
                        continue;

                    var neighbourPos = isPeak ? TriangularMath.GetPeakNeighbour(pos, i) : TriangularMath.GetValleyNeighbour(pos, i);
                    if (!cellsDictionary.TryGetValue(neighbourPos, out var neighbourData) || !(neighbourData.IsExit & neighbourData.IsPassable))
                        continue;

                    var targetDistance = TriangularMath.CalculateDistance(neighbourPos, farEndPos);
                    if (targetDistance < minNextPosDistance)
                    {
                        nextNeighbourPos = closestOtherHexPos;
                        minNextPosDistance = targetDistance;
                    }
                }

                var distance = TriangularMath.CalculateDistance(startPos, pos) + minNextPosDistance;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestThisHexPos = pos;
                    closestOtherHexPos = nextNeighbourPos;
                }
                transitionsFound++;
            }

            if (transitionsFound == 0)
            {
                #if UNITY_EDITOR
                Debug.Log("no transitions found");
                #endif
                return Result.Failed;
            }
                
            return new Result(closestThisHexPos, closestOtherHexPos);
        }

        private static void FulfillEdgesList(
            HexPathNodeKey hexNode,
            Dictionary<IntTriangularPos, EdgeCellData> dict, 
            INavigationMap map,
            bool isExit)
        {
            var trianglesPerEdge = map.TrianglesPerHexEdge;
            var hexPos = new NavigationHexPosition(hexNode, map);

            switch (hexNode.Edge)
            {
                case HexEdge.TopRight:
                    {
                        SetEdgeTransitionTrianglesList<TopRightEdgeEnumerationLogic>(new(trianglesPerEdge, hexPos), dict, map, isExit);
                        break;
                    }
                case HexEdge.BottomRight:
                    {
                        SetEdgeTransitionTrianglesList<BottomRightEdgeEnumerationLogic>(new(trianglesPerEdge, hexPos), dict, map, isExit);
                        break;
                    }
                case HexEdge.Bottom:
                    {
                        SetEdgeTransitionTrianglesList<BottomEdgeEnumerationLogic>(new(trianglesPerEdge, hexPos), dict, map, isExit);
                        break;
                    }
                case HexEdge.BottomLeft:
                    {
                        SetEdgeTransitionTrianglesList<BottomLeftEdgeEnumerationLogic>(new(trianglesPerEdge, hexPos), dict, map, isExit);
                        break;
                    }
                case HexEdge.TopLeft:
                    {
                        SetEdgeTransitionTrianglesList<TopLeftEdgeEnumerationLogic>(new(trianglesPerEdge, hexPos), dict, map, isExit);
                        break;
                    }
                default:
                    {
                        SetEdgeTransitionTrianglesList<TopEdgeEnumerationLogic>(new(trianglesPerEdge, hexPos), dict, map, isExit);
                        break;
                    }
            }
        }

        private static void SetEdgeTransitionTrianglesList<T>(EdgeEnumerator<T> enumerator, Dictionary<IntTriangularPos, EdgeCellData> dict, INavigationMap map, bool isExit) where T : struct, IEdgeEnumerationLogic
        {
            foreach (var edgePos in enumerator)
            {
                var pos = edgePos;
                dict.Add(pos, new() { IsExit = isExit, IsPassable = map.IsTrianglePassable(pos) });
            }
        }
    }
}
