using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class GetSuitablePathKeyCommand
    {
        public enum HexPathSearchResult : byte
        {
            NoPathFound,
            PointsAreInSameHex,
            PathFound
        }
        // add "Best path found" if all paths are presented?

        public struct HexPathSearchResultData
        {
            public int PathId;
            public int2 StartHex;
            public HexEdgesMask StartEdgesMask;
            public int2 EndHex;
            public HexEdgesMask EndEdgesMask;
            public int ListVersion;
        }

        public static HexPathSearchResult TryGetClosestPath(
            float3 startPos, 
            float3 endPos,
            INavigationMap map, 
            NavigationHexPathsList pathsList, 
            out HexPathSearchResultData resultData,
            bool requestPathBuilding = true)
        {
            var startHex = HexMath.DefineHex(startPos.xz, map.HexEdgeSize);
            var endHex = HexMath.DefineHex(endPos.xz, map.HexEdgeSize);

            resultData = new();
            resultData.StartHex = startHex;
            resultData.EndHex = endHex;
            resultData.ListVersion = pathsList.Version;

            // edges that accessible from this world points
            var startEdgesMask = DefineAccessibleEdges(startPos, map);
            var endEdgesMask = DefineAccessibleEdges(endPos, map);

            if (math.all(startHex == endHex) & startEdgesMask.HasOverlapsWith(endEdgesMask))
            {
                return HexPathSearchResult.PointsAreInSameHex;
            }

            var pathsCount = 0;
            Span<PathShortData> pathsData = stackalloc PathShortData[36];            

            for (var startEdge = 0; startEdge < 6; startEdge++)
            {
                if (startEdgesMask.IsEdgePresented(startEdge))
                {
                    for (var endEdge = 0; endEdge < 6; endEdge++)
                    {
                        if (endEdgesMask.IsEdgePresented(endEdge))
                        {
                            var startKey = new HexPathNodeKey(startHex, startEdge);
                            var endKey = new HexPathNodeKey(endHex, endEdge);

                            if (pathsList.TryGetPathShortData(
                                    start: startKey,
                                    end: endKey,
                                    out var shortPathData))
                            {
                                pathsData[pathsCount++] = shortPathData;
                            }
                            else
                            {
                                if (requestPathBuilding)
                                    pathsList.RequestPathBuilding(startKey, endKey);
                            }
                        }
                    }
                }
            }

            resultData.StartEdgesMask = startEdgesMask;
            resultData.EndEdgesMask = endEdgesMask;

            if (pathsCount == 0)
            {
                resultData.PathId = -1;
                return HexPathSearchResult.NoPathFound;
            }

            if (pathsCount == 1)
            {
                resultData.PathId = pathsData[0].PathId;
                return HexPathSearchResult.PathFound;
            }
                
            var minLength = pathsData[0].PathLength;
            var shortestPathId = pathsData[0].PathId;

            for (var i = 1; i< pathsCount; i++)
            {
                var path = pathsData[i];
                if (path.PathLength < minLength)
                {
                    minLength = path.PathLength;
                    shortestPathId = path.PathId;
                }
            }

            resultData.PathId = shortestPathId;
            return HexPathSearchResult.PathFound;
        }
    
        private static HexEdgesMask DefineAccessibleEdges(float3 worldPos, INavigationMap map)
        {
            var hexCoord = HexMath.DefineHex(worldPos.xz, map.HexEdgeSize);
            var flowMap = map.GetFlowMap(hexCoord);
            var triangularPos = TriangularMath.WorldToTrianglePos(worldPos, map.TriangleEdgeSize);
            var cellData = flowMap.GetCombinedCellData(triangularPos);
            var edgesAccessMask = cellData.GetCombinedEdgeAccessMask();
            return new HexEdgesMask(edgesAccessMask);
        }
    }
}
