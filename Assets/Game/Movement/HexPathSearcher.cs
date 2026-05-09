using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public enum HexPathSearchResult : byte
    {
        NoPathFound,
        PointsAreInSameHex,
        PathFound,
        SingleEdgePass
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
        public int PathNodesCount;
        public HexEdge ExitEdge;
    }

    public class HexPathSearcher
    {
        private readonly HexPathShortData[] _cacheArray = new HexPathShortData[MAX_PATHS_COUNT];
        private readonly INavigationMap _map;
        private readonly NavigationHexPathsList _hexPathsList;

        private const int MAX_PATHS_COUNT = 36;

        

        public HexPathSearcher(INavigationMap map, NavigationHexPathsList hexPathsList)
        {
            _map = map;
            _hexPathsList = hexPathsList;
        }

        public HexPathSearchResult TryGetShortestPath(
            float3 startPos,
            float3 endPos,
            out HexPathSearchResultData resultData,
            bool requestPathBuilding = true)
        {
            var startHex = HexMath.DefineHex(startPos.xz, _map.HexEdgeLength);
            var endHex = HexMath.DefineHex(endPos.xz, _map.HexEdgeLength);

            resultData = new();
            resultData.StartHex = startHex;
            resultData.EndHex = endHex;
            resultData.ListVersion = _hexPathsList.Version;

            // edges that accessible from this world points
            var startEdgesMask = DefineAccessibleEdges(startPos);
            var endEdgesMask = DefineAccessibleEdges(endPos);

            if (math.all(startHex == endHex) && startEdgesMask.HasOverlapsWith(endEdgesMask))
            {
                return HexPathSearchResult.PointsAreInSameHex;
            }

            if (HexMath.AreNeighbours(startHex, endHex))
            {
                var offsetEdge = HexMath.HexOffsetVectorToEdge(math.sign(endHex - startHex));                
                if (startEdgesMask.IsEdgePresented(offsetEdge) && endEdgesMask.IsEdgePresented(offsetEdge.ToOpposite()))
                {
                    // hexes are neighbours and there is direct edge passage
                    resultData.ExitEdge = offsetEdge;
                    resultData.PathNodesCount = 1;
                    return HexPathSearchResult.SingleEdgePass;
                }
            }

            var pathsCount = 0;
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

                            if (_hexPathsList.TryGetPathShortData(
                                    start: startKey,
                                    end: endKey,
                                    out var shortPathData))
                            {
                                _cacheArray[pathsCount++] = shortPathData;
                            }
                            else
                            {
                                if (requestPathBuilding)
                                    _hexPathsList.RequestPathBuilding(startKey, endKey);
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
                var path = _cacheArray[0];
                resultData.PathId = path.PathId;
                resultData.PathNodesCount = path.PathNodesCount;
                return HexPathSearchResult.PathFound;
            }

            var minLength = _cacheArray[0].PathLength;
            var shortestPathId = _cacheArray[0].PathId;

            for (var i = 1; i < pathsCount; i++)
            {
                var path = _cacheArray[i];
                if (path.PathLength < minLength)
                {
                    minLength = path.PathLength;
                    shortestPathId = path.PathId;
                }
            }

            resultData.PathId = shortestPathId;
            return HexPathSearchResult.PathFound;
        }

        private HexEdgesMask DefineAccessibleEdges(float3 worldPos)
        {
            var hexCoord = HexMath.DefineHex(worldPos.xz, _map.HexEdgeLength);
            var triangularPos = TriangularMath.WorldToTrianglePos(worldPos, _map.TriangleHeight);
            return _map.GetFlowData(triangularPos).GetCombinedEdgeAccessMask();
        }
    }
}
