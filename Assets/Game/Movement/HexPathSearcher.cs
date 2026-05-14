using System;
using System.Collections.Generic;
using ZE.MechBattle.Navigation;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class HexPathSearcher
    {
        public enum HexPathSearchResult : byte
        {
            PathImpossible, NotAllOptionsCalculated, OnlyIncompletePathPossible, PathFound
        }

        public struct HexPathSearchResultData
        {
            public HexPathSearchResult Result;
            public HexPathNodeKey EndNode;
            public int PathId;
            public int NodesCount;

            public HexPathSearchResultData(HexPathSearchResult result, HexPathOption option)
            {
                Result = result;

                if (option.IsValid)
                {
                    EndNode = option.LastNode;
                    PathId = option.PathId;
                    NodesCount = option.NodesCount;
                }
                else
                {
                    EndNode = default;
                    PathId = -1;
                    NodesCount = 0;
                }
            }
        }

        private readonly INavigationMap _map;
        private readonly RequestedHexPathsList _requestedPathsList;
        private readonly HexPathsLRUBuffer _pathsList;
        private readonly LRUDictionaryCache<CachedHexPathSearchKey, HexPathSearchResultData> _cachedResults;
        private HashSet<HexPathNodeKey> _transitionableNodes;

        public HexPathSearcher(
            INavigationMap map, 
            RequestedHexPathsList requestedPathsList, 
            HexPathsLRUBuffer hexPaths,
            int cacheLimit)
        {
            _map = map;
            _requestedPathsList = requestedPathsList;
            _pathsList = hexPaths;
            _cachedResults = new(cacheLimit);
        }

        public void OnMapVersionChanged()
        {
            _transitionableNodes = GetHexTransitionableNodesCommand.Execute(_map, checkEdgesPassability: true);
            _cachedResults.Clear();
        }

        public void LeaveOnlyCalculatedPathsInCache()
        {
            var length = _cachedResults.Length;
            if (length == 0)
                return;

            // remove not fully calculated cached results, because they may be calculated on next frame
            var removeKeys = new List<CachedHexPathSearchKey>(capacity: length);
            foreach (var kvp in _cachedResults)
            {
                if (kvp.Value.Result == HexPathSearchResult.NotAllOptionsCalculated)
                    removeKeys.Add(kvp.Key);
            }

            foreach (var key in removeKeys)
            {
                _cachedResults.Remove(key);
            }
        }

        public HexPathSearchResultData GetHexPathData(int2 startHexCoord, HexEdgesMask startEdgesMask, int2 endHexCoord, HexEdgesMask endEdgesMask,  bool requestMissedPathsCalculation = true)
        {
            var operationKey = new CachedHexPathSearchKey(startHexCoord, startEdgesMask, endHexCoord, endEdgesMask);
            if (_cachedResults.TryGetCachedValue(operationKey, out var cachedResult))
                return cachedResult;


            HexPathOption shortestPathOption = HexPathOption.Default;
            HexPathOption shortestPathReachedOption = HexPathOption.Default;            
            var allOptionsCalculated = true;

            for (var startEdge = 0; startEdge < 6; startEdge++)
            {
                if (!startEdgesMask.IsEdgePresented(startEdge) || !_transitionableNodes.Contains(new(startHexCoord, startEdge)))
                    continue;

                for (var endEdge = 0; endEdge < 6; endEdge++)
                {
                    if (!endEdgesMask.IsEdgePresented(endEdge) || !_transitionableNodes.Contains(new(endHexCoord, endEdge)))
                        continue;

                    var startNode = new HexPathNodeKey(startHexCoord, startEdge);
                    var endNode = new HexPathNodeKey(endHexCoord, endEdge);
                    if (_pathsList.TryGetPathByEndpoints(startNode, endNode, out var path))
                    {
                        if (!path.IsCalculated)
                        {
                            allOptionsCalculated = false;
                            continue;
                        }

                        var newPathCost = path.PathCost;

                        if (path.HasReachedTarget
                            && newPathCost < shortestPathReachedOption.PathCost)
                                shortestPathReachedOption = new(path.Id, path);

                        if (newPathCost < shortestPathOption.PathCost)
                            shortestPathOption = new(path.Id, path);

                    }
                    else
                    {
                        allOptionsCalculated = false;
                        _requestedPathsList.Add(new(startNode, endNode));
                        //UnityEngine.Debug.Log($"not found: {startNode}->{endNode}");
                    }                    
                }
            }

            HexPathSearchResultData resultData;
            if (!allOptionsCalculated)
            {
                resultData = new (HexPathSearchResult.NotAllOptionsCalculated, shortestPathOption);
            }
            else
            {
                if (shortestPathReachedOption.IsValid)
                {
                    resultData = new(HexPathSearchResult.PathFound, shortestPathReachedOption);
                }
                else
                {
                    if (shortestPathOption.IsValid) 
                        resultData = new(HexPathSearchResult.OnlyIncompletePathPossible, shortestPathOption);
                    else
                        resultData = new()
                        {
                            Result = HexPathSearchResult.PathImpossible
                        };
                }
            }

            //UnityEngine.Debug.Log($"{shortestPathOption.IsValid} : {resultData.Result}");

            _cachedResults.AddCachedValue(operationKey, resultData);
            return resultData;
        }
    
    }
}
