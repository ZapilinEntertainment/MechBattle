using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation
{
    public static class GetShortestHexPathCommand
    {
        public struct PathfindResult
        {
            public readonly bool IsSuccess;
            public readonly List<HexPathNodeKey> Path;

            public PathfindResult(List<HexPathNodeKey> path)
            {
                IsSuccess = true;
                Path = path;
            }

            public static PathfindResult Failed => new();
        }

        public static async Task<PathfindResult> ExecuteAsync(
           IntTriangularPos start,
           IntTriangularPos end,
           INavigationMap map,
           HexPathJobCollections hexPathJobData,
           CancellationToken cancellationToken)
        {
            var startHex = TriangularMath.TriangularToHex(start, map.TriangleHeight, map.HexEdgeSize);
            var endHex = TriangularMath.TriangularToHex(end, map.TriangleHeight, map.HexEdgeSize);

            var startHexFlowMap = map.GetFlowMap(startHex);
            var endHexFlowMap = map.GetFlowMap(endHex);

            var startHexAccessMap = startHexFlowMap.GetAccessMap();
            var endHexAccessMap = endHexFlowMap.GetAccessMap();

            var startPointEdgesAccessMask = startHexFlowMap.GetCombinedCellData(start).GetCombinedEdgeAccessMask();
            var endPointEdgesAccessMask = endHexFlowMap.GetCombinedCellData(end).GetCombinedEdgeAccessMask();

            var calculatedPaths = 0;
            var minCost = float.MaxValue;
            var results = new List<HexPathNodeKey>();

            for (var startEdge = 0; startEdge < 6; startEdge++)
            {
                if (!startPointEdgesAccessMask.IsEdgePresented(startEdge)
                    || !startHexAccessMap.IsEdgePassable(startEdge))
                    continue;

                for (var endEdge = 0; endEdge < 6; endEdge++)
                {
                    if (!endPointEdgesAccessMask.IsEdgePresented(endEdge)
                        || !endHexAccessMap.IsEdgePassable(endEdge))
                        continue;

                    var hexPathJob = new ConstructHexPathJob()
                    {
                        HexData = hexPathJobData.HexData,
                        NavigationData = hexPathJobData.NavigationData,
                        ResultingData = hexPathJobData.ResultingData,
                        OpenedList = hexPathJobData.OpenedList,
                        PathCost = hexPathJobData.PathCost,

                        Start = new(startHex, startEdge),
                        End = new(endHex, endEdge)
                    };
                    var handle = hexPathJob.ScheduleByRef();
                    while (!handle.IsCompleted)
                    {
                        await Task.Delay(100);
                    }
                    handle.Complete();
                    if (cancellationToken.IsCancellationRequested)
                        return default;

                    if (hexPathJob.PathCost.Value < minCost)
                    {
                        minCost = hexPathJob.PathCost.Value;
                        results.Clear();
                        var resultedPath = hexPathJob.ResultingData;
                        for (var i = 0; i < resultedPath.Length; i++)
                        {
                            results.Add(resultedPath[i]);
                        }
                    }

                    calculatedPaths++;
                }
            }

            if (calculatedPaths == 0)
                return PathfindResult.Failed;

            return new(results);
        }
    }
}
