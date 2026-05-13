using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation
{
    // IMPORTANT: this is a debug search command 
    // TODO: Merge logics
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

            private readonly int DirectHexDistance;
            private readonly HexEdge DirectPathStartEdge;

            private readonly int2 StartHexCoord;
            private readonly int2 EndHexCoord;

            private readonly HexPathJobCollections _hexPathJobData;

            private readonly HexEdgesAccessMap StartHexAccessMap;
            private readonly HexEdgesAccessMap EndHexAccessMap;

            private readonly HexEdgesMask StartEdgesPassability;
            private readonly HexEdgesMask EndEdgesPassability;

            private readonly HexEdgesMask StartPointEdgesAccessMask;
            private readonly HexEdgesMask EndPointEdgesAccessMask;
            

            public Logic(HexPathJobCollections hexPathJobData, INavigationMap map, IntTriangularPos start, IntTriangularPos end)
            {
                _hexPathJobData = hexPathJobData;

                StartHexCoord = TriangularMath.TriangularToHex(start, map.TriangleHeight, map.HexEdgeLength);
                EndHexCoord = TriangularMath.TriangularToHex(end, map.TriangleHeight, map.HexEdgeLength);
                
                DirectHexDistance = HexMath.CalculateHexPosDistance(StartHexCoord, EndHexCoord);
                var dir = math.sign(EndHexCoord - StartHexCoord);
                DirectPathStartEdge = HexMath.HexOffsetVectorToEdge(dir);

                var startHex = map.GetOrCreateHex(StartHexCoord);
                var endHex = map.GetOrCreateHex(EndHexCoord);

                StartHexAccessMap = startHex.AccessMap;
                EndHexAccessMap = endHex.AccessMap;

                StartEdgesPassability = startHex.EdgesPassability;
                EndEdgesPassability = endHex.EdgesPassability;

                StartPointEdgesAccessMask = map.GetFlowData(start).GetCombinedEdgeAccessMask();
                EndPointEdgesAccessMask = map.GetFlowData(end).GetCombinedEdgeAccessMask();

                Results = new List<HexPathNodeKey>();
            }

            public bool IsStartEdgePassable(int edgeIndex) => StartEdgesPassability.IsEdgePresented(edgeIndex);
            public bool IsEndEdgePassable(int edgeIndex) => EndEdgesPassability.IsEdgePresented(edgeIndex);


            public bool TryConstructJob(int startEdgeIndex, int endEdgeIndex, float currentMinCost, out ConstructHexPathJob job)
            {
                var startHexEdge = (HexEdge)startEdgeIndex;
                var endHexEdge = (HexEdge) endEdgeIndex;
                var directPathLength = DirectHexDistance + HexMath.GetDelta(startHexEdge, DirectPathStartEdge) + HexMath.GetDelta(endHexEdge, DirectPathStartEdge);

                if (directPathLength > currentMinCost)
                {
                    job = default;
                    return false;
                }                    

                job = new ConstructHexPathJob()
                {
                    HexData = _hexPathJobData.HexData,
                    NavigationData = _hexPathJobData.NavigationData,
                    ResultingData = _hexPathJobData.ResultingData,
                    OpenedList = _hexPathJobData.OpenedList,
                    PathCost = _hexPathJobData.PathCost,

                    Start = new(StartHexCoord, startEdgeIndex),
                    End = new(EndHexCoord, endEdgeIndex)
                };
                return true;
            }
                

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

            public bool CheckForOneStepPath(out HexPathNodeKey node)
            {
                if (DirectHexDistance != 1)
                {
                    node = default;
                    return false;
                }
                
                node = new(StartHexCoord, DirectPathStartEdge);
                return IsStartEdgePassable((int)DirectPathStartEdge) && IsEndEdgePassable((int)(DirectPathStartEdge.ToOpposite()));
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
            if (logic.CheckForOneStepPath(out var singleNode))
                return new(new() { singleNode });

            var minCost = float.MaxValue;
            var calculatedPaths = 0;

            for (var startEdge = 0; startEdge < 6; startEdge++)
            {
                if (!logic.IsStartEdgePassable(startEdge))
                    continue;

                for (var endEdge = 0; endEdge < 6; endEdge++)
                {
                    if (!logic.IsEndEdgePassable(endEdge))
                        continue;

                    if (!logic.TryConstructJob(startEdge, endEdge, minCost, out var job))
                        continue;

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
            if (logic.CheckForOneStepPath(out var singleNode))
                return new(new() { singleNode });

            var calculatedPaths = 0;
            var minCost = float.MaxValue;
            var shortestPathFound = false;

            for (var startEdge = 0; startEdge < 6; startEdge++)
            {
                if (!logic.IsStartEdgePassable(startEdge))
                    continue;

                for (var endEdge = 0; endEdge < 6; endEdge++)
                {
                    if (!logic.IsEndEdgePassable(endEdge))
                        continue;

                    if (!logic.TryConstructJob(startEdge, endEdge, minCost, out var job))
                        continue;

                    var handle = job.ScheduleByRef();
                    handle.Complete();

                    minCost = logic.CheckJobResults(job, minCost);
                    calculatedPaths++;
                }

                if (shortestPathFound)
                    break;
            }

            Debug.Log($"calculatedPaths: {calculatedPaths}, minCost: {minCost} of path: {logic.Results[0]} -> {logic.Results[logic.Results.Count - 1]}");
            
            if (calculatedPaths == 0)
                return PathfindResult.Failed;

            return new(logic.Results);
        }
    }
}
