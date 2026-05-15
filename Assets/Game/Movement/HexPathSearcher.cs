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

        private readonly float _edgesCostDivideCf;
        private readonly INavigationMap _map;
        private readonly RequestedHexPathsList _requestedPathsList;
        private readonly HexPathsLRUBuffer _pathsList;
        private HexTransitionableNodes _transitionableNodes;

        public HexPathSearcher(
            INavigationMap map, 
            RequestedHexPathsList requestedPathsList, 
            HexPathsLRUBuffer hexPaths,
            int cacheLimit)
        {
            _map = map;
            _edgesCostDivideCf = 1f / (_map.TrianglesPerHexEdge  * 2f);
            _requestedPathsList = requestedPathsList;
            _pathsList = hexPaths;
        }

        public void OnMapVersionChanged()
        {
            _transitionableNodes = GetHexTransitionableNodesCommand.Execute(_map, checkEdgesPassability: true);
        }

        public HexPathSearchResultData GetHexPathData(
            in HexPathSearchRequest request,
            bool requestMissedPathsCalculation = true)
        {

            HexPathOption shortestPathOption = HexPathOption.Default;
            HexPathOption shortestPathReachedOption = HexPathOption.Default;            
            var allOptionsCalculated = true;
            CalculateExitCosts(request, out var startEdgesCost, out var endEdgesCost);

            var startHexCoord = request.StartHexCoord;
            var endHexCoord = request.EndHexCoord;

            for (var startEdge = 0; startEdge < 6; startEdge++)
            {
                if (!request.StartEdgesMask.IsEdgePresented(startEdge) || !_transitionableNodes.IsNodeTransitionable(startHexCoord, startEdge))
                    continue;

                for (var endEdge = 0; endEdge < 6; endEdge++)
                {
                    if (!request.EndEdgesMask.IsEdgePresented(endEdge) || !_transitionableNodes.IsNodeTransitionable(endHexCoord, endEdge))
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

                        var startEdgeCost = startEdgesCost[startEdge];
                        var endEdgeCost = endEdgesCost[endEdge];
                        var newPathCost = path.PathCost + startEdgeCost + endEdgeCost;

                        if (path.HasReachedTarget
                            && newPathCost < shortestPathReachedOption.FullPathCost)
                                shortestPathReachedOption = new(path.Id, path, startEdgeCost, endEdgeCost);

                        if (newPathCost < shortestPathOption.FullPathCost)
                            shortestPathOption = new(path.Id, path, startEdgeCost, endEdgeCost);

                    }
                    else
                    {
                        allOptionsCalculated = false;
                        _requestedPathsList.Add(new(startNode, endNode));
                        //UnityEngine.Debug.Log($"not found: {startNode}->{endNode}");
                    }                    
                }

                if (!allOptionsCalculated)
                    break;
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
            return resultData;
        }
    
        private void CalculateExitCosts(in HexPathSearchRequest request, out float6 startEdgesCost, out float6 endEdgesCost)
        {
            startEdgesCost = new();
            endEdgesCost = new();
            var directionsCf = HexTransitionLogic.GetDirectionCostCoefficients(request.StartHexCoord, request.EndHexCoord);

            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                startEdgesCost[i] = request.StartPosEdgeDistances[edge] * _edgesCostDivideCf * (1 - directionsCf[edge]);
                endEdgesCost[i] = request.EndPosEdgeDistances[edge] * _edgesCostDivideCf * (1 + directionsCf[edge]);
            }

           
        }
    }
}
