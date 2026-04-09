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

        private readonly struct Logic
        {
            public readonly List<HexPathNodeKey> Results;
            private readonly HexPathJobCollections _hexPathJobData;

            private readonly int2 StartHexCoord;
            private readonly int2 EndHexCoord;

            private readonly HexEdgesAccessMap StartHexAccessMap;
            private readonly HexEdgesAccessMap EndHexAccessMap;

            private readonly HexEdgesMask StartPointEdgesAccessMask;
            private readonly HexEdgesMask EndPointEdgesAccessMask;
            

            public Logic(HexPathJobCollections hexPathJobData, INavigationMap map, IntTriangularPos start, IntTriangularPos end)
            {
                _hexPathJobData = hexPathJobData;

                StartHexCoord = TriangularMath.TriangularToHex(start, map.TriangleHeight, map.HexEdgeSize);
                EndHexCoord = TriangularMath.TriangularToHex(end, map.TriangleHeight, map.HexEdgeSize);

                var startHexFlowMap = map.GetFlowMap(StartHexCoord);
                var endHexFlowMap = map.GetFlowMap(EndHexCoord);

                StartHexAccessMap = startHexFlowMap.GetAccessMap();
                EndHexAccessMap = endHexFlowMap.GetAccessMap();

                StartPointEdgesAccessMask = startHexFlowMap.GetCombinedCellData(start).GetCombinedEdgeAccessMask();
                EndPointEdgesAccessMask = endHexFlowMap.GetCombinedCellData(end).GetCombinedEdgeAccessMask();

                Results = new List<HexPathNodeKey>();
            }

            public bool IsStartEdgeOperable(int edgeIndex) => 
                StartPointEdgesAccessMask.IsEdgePresented(edgeIndex)
                && StartHexAccessMap.IsEdgePassable(edgeIndex);

            public bool IsEndEdgeOperable(int edgeIndex) =>
                EndPointEdgesAccessMask.IsEdgePresented(edgeIndex)
                && EndHexAccessMap.IsEdgePassable(edgeIndex);

            public ConstructHexPathJob ConstructJob(int startEdgeIndex, int endEdgeIndex) =>
                new ConstructHexPathJob()
                {
                    HexData = _hexPathJobData.HexData,
                    NavigationData = _hexPathJobData.NavigationData,
                    ResultingData = _hexPathJobData.ResultingData,
                    OpenedList = _hexPathJobData.OpenedList,
                    PathCost = _hexPathJobData.PathCost,

                    Start = new(StartHexCoord, startEdgeIndex),
                    End = new(EndHexCoord, endEdgeIndex)
                };

            public float CheckJobResults(in ConstructHexPathJob job, float minCost)
            { 
                if (job.PathCost.Value < minCost)
                {
                    minCost = job.PathCost.Value;
                    Results.Clear();
                    var resultedPath = job.ResultingData;
                    for (var i = 0; i < resultedPath.Length; i++)
                    {
                        Results.Add(resultedPath[i]);
                    }
                }
                return minCost;
            }
        }

        public static async Task<PathfindResult> ExecuteAsync(
           IntTriangularPos start,
           IntTriangularPos end,
           INavigationMap map,
           HexPathJobCollections hexPathJobData,
           CancellationToken cancellationToken)
        {
            var logic = new Logic(hexPathJobData, map, start,end);
            var minCost = float.MaxValue;
            var calculatedPaths = 0;

            for (var startEdge = 0; startEdge < 6; startEdge++)
            {
                if (!logic.IsStartEdgeOperable(startEdge))
                    continue;

                for (var endEdge = 0; endEdge < 6; endEdge++)
                {
                    if (!logic.IsEndEdgeOperable(endEdge))
                        continue;

                    var job = logic.ConstructJob(startEdge, endEdge);
                    var handle = job.ScheduleByRef();
                    while (!handle.IsCompleted)
                    {
                        await Task.Delay(100);
                    }
                    handle.Complete();
                    if (cancellationToken.IsCancellationRequested)
                        return default;

                    minCost = logic.CheckJobResults(job, minCost);

                    calculatedPaths++;
                }
            }

            if (calculatedPaths == 0)
                return PathfindResult.Failed;

            return new(logic.Results);
        }

        public static PathfindResult Execute(
          IntTriangularPos start,
          IntTriangularPos end,
          INavigationMap map,
          HexPathJobCollections hexPathJobData)
        {
            var logic = new Logic(hexPathJobData, map, start, end);

            var calculatedPaths = 0;
            var minCost = float.MaxValue;

            for (var startEdge = 0; startEdge < 6; startEdge++)
            {
                if (!logic.IsStartEdgeOperable(startEdge))
                    continue;

                for (var endEdge = 0; endEdge < 6; endEdge++)
                {
                    if (!logic.IsEndEdgeOperable(endEdge))
                        continue;

                    var job = logic.ConstructJob(startEdge, endEdge);
                    var handle = job.ScheduleByRef();
                    handle.Complete();

                    minCost = logic.CheckJobResults(job, minCost);
                    calculatedPaths++;
                }
            }

            if (calculatedPaths == 0)
                return PathfindResult.Failed;

            return new(logic.Results);
        }
    }
}
