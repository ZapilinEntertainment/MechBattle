using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public abstract class TriangularPathBuilderBase : IDisposable
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

        protected readonly struct RoutePlanPoint
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

        protected readonly INavigationMap _map;
        protected TriangularPathJobCollections _triangularPathJobData;
        protected HexPathJobCollections _hexPathJobData;

        public TriangularPathBuilderBase(INavigationMap map)
        {
            _map = map;
        }

        public virtual void Dispose()
        {
            FinalDispose();
        }

        protected void FinalDispose()
        {
            _hexPathJobData?.Dispose();
            _triangularPathJobData?.Dispose();
        }

        protected IntTriangularPos AddPathTriangles(IntTriangularPos startPos, HexPathNodeKey exitNode, List<IntTriangularPos> points)
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
               
                exitFound = cellData.ExitDistance == 0;
                if (exitFound)
                {

                }
                else
                {
                    if (pos.IsPeak)
                        pos = TriangularMath.GetPeakNeighbour(pos, flowDirection);
                    else
                        pos = TriangularMath.GetValleyNeighbour(pos, flowDirection);
                }

                points.Add(pos);
                operationsCount++;
            }

            return pos;
        }

        protected IntTriangularPos GetEdgeCenter(HexPathNodeKey node) => node.Edge.GetEdgeCenterPos(new NavigationHexPosition(node, _map).TriangularCenterPos, _map.TrianglesPerHexEdge);

        protected HexPathJobCollections GetHexPathJobData()
        {
            _hexPathJobData ??= PrepareHexPathJobCollectionsCommand.Execute(Allocator.Persistent, _map);
            return _hexPathJobData;
        }

        protected void CalculateTrianglePath(int2 hexPos, IntTriangularPos start, IntTriangularPos end)
        {
            var handle = LaunchTriangularPathJob(hexPos, start, end);
            handle.Complete();
        }



        protected JobHandle LaunchTriangularPathJob(int2 hexPos, IntTriangularPos start, IntTriangularPos end)
        {
            var jobData = GetTriangularPathJobData(hexPos);
            var job = new ConstructTriangularPathJob()
            {
                Start = start,
                End = end,
                CalculationData = jobData.CalculationData,
                PassabilityData = jobData.PassabilityData,
                OpenedList = jobData.OpenedList,
                ResultList = jobData.ResultList,
            };
            return job.ScheduleByRef();
        }

        protected TriangularPathJobCollections GetTriangularPathJobData(int2 hexCoord)
        {
            if (_triangularPathJobData == null)
                _triangularPathJobData = PrepareTriangularPathJobCollectionsCommand.Execute(
                    Allocator.Persistent,
                    CreateHexPos(hexCoord),
                    _map.Settings,
                    _map.GetFlowMap(hexCoord));

            else
                _triangularPathJobData.ChangeCenter(CreateHexPos(hexCoord));

            return _triangularPathJobData;
        }

        protected NavigationHexPosition CreateHexPos(int2 hexCoord) => new(hexCoord, _map.HexEdgeSize, _map.TrianglesPerHexEdge);

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
