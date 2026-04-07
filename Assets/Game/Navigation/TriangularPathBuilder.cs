using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public class TriangularPathBuilder : IDisposable
    {
        public readonly struct Result
        {
            public bool IsSucceed => ResultCode == ResultCode.Succeed;
            public readonly ResultCode ResultCode;
            public readonly List<IntTriangularPos> Points;

            public Result(List<IntTriangularPos> points)
            {
                Points = points;
                ResultCode = ResultCode.Succeed;
            }

            public Result(ResultCode notSucceedCode)
            {
                Points = null;
                ResultCode = notSucceedCode;
            }
        }

        public enum ResultCode : byte
        {
            Undefined,
            Failed,
            Succeed,
            CannotBuildHexPath,
            InvalidHexPath,
            InvalidTransition
        }

        private readonly struct RoutePlanPoint
        {
            public readonly IntTriangularPos Start;
            public readonly IntTriangularPos End;
            public readonly bool UseFlowMapToConnect;

            public RoutePlanPoint(IntTriangularPos start, IntTriangularPos end, bool useFlowMapToConnect)
            {
                Start = start;
                End = end;
                UseFlowMapToConnect = useFlowMapToConnect;
            }
        }

        private readonly INavigationMap _map;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private bool _isJobExecuting = false;
        private TriangularPathJobCollections _triangularPathJobData;
        private HexPathJobCollections _hexPathJobData;

        public TriangularPathBuilder(INavigationMap map)
        {
            _map = map;
        }

        public async Task<Result> Build(
            IntTriangularPos startPos, 
            IntTriangularPos endPos,
            CancellationToken cancellationToken)
        {
            var points = new List<IntTriangularPos>();
            var startHex = TriangularMath.TriangularToHex(startPos, _map.TriangleHeight, _map.HexEdgeSize);
            var endHex = TriangularMath.TriangularToHex(endPos, _map.TriangleHeight, _map.HexEdgeSize);

            if (math.all(startHex == endHex))
            {
                //single hex
                CalculateTrianglePath(startHex, startPos, endPos);

                /*  DEBUG LOG
                var coordsConverter = _triangularPathJobData.SetupData.CoordsConverter;
                for (var j = 0; j < _triangularPathJobData.CalculationData.Length; j++)
                {
                    var setupData = _triangularPathJobData.SetupData[j];
                    if (!setupData.IsValid)
                        continue;

                    Debug.Log($"{coordsConverter.IndexToTriangular(j)}: {_triangularPathJobData.CalculationData[j].PathCost}");
                }
                */

                foreach (var point in _triangularPathJobData.ResultList)
                {
                    points.Add(point);
                }

                return new(points);
            }

            // CALCULATE HEX PATH   
            _isJobExecuting = true;
            GetShortestHexPathCommand.PathfindResult result = default;
            using var combinedSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            CancellationToken combinedToken = combinedSource.Token;
            try            
            {                
                result = await GetShortestHexPathCommand.ExecuteAsync(startPos, endPos, _map, GetHexPathJobData(), combinedToken);
                combinedSource.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogError("hex pathfinding failed: " + ex.ToString());
            }
            finally
            {
                _isJobExecuting = false;
            }

            if (combinedToken.IsCancellationRequested)
                return default;

            if (!result.IsSuccess)
                return new(ResultCode.CannotBuildHexPath);

            var hexNodes = result.Path;
            var transitionsCount = hexNodes.Count;
            if (transitionsCount == 0)
                return new(ResultCode.InvalidHexPath);

            var prevPos = startPos;
            for (var i = 0; i < hexNodes.Count; i++)
            {
                prevPos = AddPathTriangles(prevPos, hexNodes[i], points);
            }


            
            // last part: edge transition (inside last hex) -> final pos
            _isJobExecuting = true;
            try
            {
                CalculateTrianglePath(endHex, prevPos, endPos);
            }
            catch (Exception ex)
            {
                Debug.LogError("triangle pathfinding failed: " + ex.ToString());
            }
            finally
            {
                _isJobExecuting = false;
            }

            if (combinedToken.IsCancellationRequested)
                return default;

            var count = _triangularPathJobData.ResultList.Length;
            for (var i = 1; i < count; i++)
            {
                points.Add(_triangularPathJobData.ResultList[i]);
            }

            return new(points);
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            if (_isJobExecuting)
                DisposeAsync();
            else
                FinalDispose();
        }

        private async void DisposeAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                Debug.LogWarning("job calculations still not finished, waiting for complete...");
                var token = cts.Token;
                do
                {
                    await Awaitable.NextFrameAsync();
                }
                while (_isJobExecuting & !token.IsCancellationRequested);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("dispose timeout! Did you forget to set async flag to false?");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                FinalDispose();
                Debug.LogWarning("triangular path builder disposed");
            }
        }

        private void FinalDispose()
        {
            _hexPathJobData?.Dispose();
            _triangularPathJobData?.Dispose();
        }

        private IntTriangularPos AddPathTriangles(IntTriangularPos startPos, HexPathNodeKey exitNode, List<IntTriangularPos> points)
        {
            var pos = startPos;
            var hexCoord = TriangularMath.TriangularToHex(pos, _map.TriangleHeight, _map.HexEdgeSize);
            var flowMap = _map.GetFlowMap(hexCoord);
            var exitFound = false;
            points.Add(pos);

            if (!math.all(exitNode.HexCoord == hexCoord))
                exitNode = exitNode.ToOpposite();

            var maxOperations = TriangularMath.GetTrianglesCountInHex(_map.TrianglesPerHexEdge);
            var operationsCount = 0;

            while (!exitFound && operationsCount < maxOperations)
            {
                var cellData = flowMap.GetCombinedCellData(pos)[exitNode.Edge];
                var flowDirection = cellData.Direction;

                if (pos.IsPeak)
                    pos = TriangularMath.GetPeakNeighbour(pos, flowDirection);
                else
                    pos = TriangularMath.GetValleyNeighbour(pos, flowDirection);

                points.Add(pos);

                exitFound = cellData.ExitDistance == 0;
                operationsCount++;
            }

            return pos;
        }

        private IntTriangularPos GetEdgeCenter(HexPathNodeKey node) => node.Edge.GetEdgeCenterPos(new NavigationHexPosition(node, _map).TriangularCenterPos, _map.TrianglesPerHexEdge);

        private HexPathJobCollections GetHexPathJobData()
        {
            _hexPathJobData ??= PrepareHexPathJobCollectionsCommand.Execute(Allocator.Persistent, _map);
            return _hexPathJobData;
        }

        private void CalculateTrianglePath(int2 hexPos, IntTriangularPos start, IntTriangularPos end)
        {
            var handle = LaunchTriangularPathJob(hexPos, start, end);
            handle.Complete();
        }

        

        private JobHandle LaunchTriangularPathJob(int2 hexPos, IntTriangularPos start, IntTriangularPos end)
        {
            var jobData = GetTriangularPathJobData(hexPos);
            var job = new ConstructTriangularPathJob()
            {
                Start = start,
                End = end,
                CalculationData = jobData.CalculationData,
                SetupData = jobData.SetupData,
                OpenedList = jobData.OpenedList,
                ResultList = jobData.ResultList,
            };
            return job.ScheduleByRef();
        }

        private TriangularPathJobCollections GetTriangularPathJobData(int2 hexCoord)
        {
            if (_triangularPathJobData == null)
                _triangularPathJobData = PrepareTriangularPathJobCollectionsCommand.Execute(
                    Allocator.Persistent,
                    CreateHexPos(hexCoord),
                    _map.TrianglesPerHexEdge,
                    _map.GetFlowMap(hexCoord));

            else
                _triangularPathJobData.ChangeCenter(CreateHexPos(hexCoord));

            return _triangularPathJobData;
        }

        private NavigationHexPosition CreateHexPos(int2 hexCoord) => new(hexCoord, _map.HexEdgeSize, _map.TrianglesPerHexEdge);

        private bool TryFormRoutePlan(IntTriangularPos startPos, IntTriangularPos endPos, IReadOnlyList<HexPathNodeKey> hexNodes, out RoutePlanPoint[] planPoints)
        {
            var singleTransition = hexNodes.Count == 1;

            planPoints = new RoutePlanPoint[hexNodes.Count / 2 + 2];
            var transitionSearchResult = TryGetHexTransitionTrianglesCommand.Execute(
                _map,
                hexNodes[0],
                startPos,
                singleTransition ? endPos : GetEdgeCenter(hexNodes[1]));

            if (!transitionSearchResult.IsSucceed)
            {
                Debug.LogError("transition failed at index 0");
                return false;
            }
            planPoints[0] = new(startPos, transitionSearchResult.End, useFlowMapToConnect: false);

            var currentStartPos = transitionSearchResult.End;
            for (var i = 1; i < hexNodes.Count; i++)
            {
                var isLastTransition = i == hexNodes.Count - 1;

                transitionSearchResult = TryGetHexTransitionTrianglesCommand.Execute(
                _map,
                hexNodes[1],
                currentStartPos,
                isLastTransition ? endPos : GetEdgeCenter(hexNodes[i + 1]));

                if (!transitionSearchResult.IsSucceed)
                {
                    Debug.LogError($"transition failed at index {i}");
                    return false;
                }

                planPoints[i] = new(currentStartPos, transitionSearchResult.End, useFlowMapToConnect: true);
                currentStartPos = transitionSearchResult.End;
            }

            planPoints[hexNodes.Count] = new(transitionSearchResult.End, endPos, useFlowMapToConnect: false);
            return true;
        }
    }
}
