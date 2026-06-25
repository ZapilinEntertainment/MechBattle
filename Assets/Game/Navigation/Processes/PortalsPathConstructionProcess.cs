using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using ZE.Utils;

namespace ZE.MechBattle.Navigation
{
    public struct PortalConstructionProcessInput
    {
        public HexPathSearchRequest Request;
        public int ReservedPathId;
    }

    public class PortalsPathConstructionProcess : AsyncProcessBase<PortalConstructionProcessInput>
    {
        private interface IPortalsList
        {
            void Clear();
            void Add(PortalOption option);
        }

        private class StartPortalsList : List<PortalOption>, IPortalsList { }
        private class EndPortalsList : Dictionary<int, PortalOption>, IPortalsList
        {
            public void Add(PortalOption option) => Add(option.PortalId, option);
        }

        private readonly struct ShortestPathOption
        {
            public readonly int PortalId;
            public readonly float Length;
            private const int INVALID_PATH_ID = -1;

            public bool IsValid => PortalId != INVALID_PATH_ID;

            private ShortestPathOption(int pathId, float length)
            {
                PortalId = pathId;
                Length = length;
            }

            public ShortestPathOption TryUpdate(int otherPathId, float otherPathLength)
            {
                if (otherPathLength < Length)
                    return new(otherPathId, otherPathLength);
                else
                    return this;
            }

            public static ShortestPathOption Default = new(pathId: INVALID_PATH_ID, length: float.MaxValue);
        }

        protected override bool IsDisposeAvailable => _isDisposeAvailable;

        private readonly INavigationMap _map;
        private readonly IHexPortalsCoordinator _portalsCoordinator;
        private readonly IPortalsLogic _portalLogic;
        private readonly IPathsList<PortalPathDestinationKey, int> _pathsBuffer;

        private readonly CalculatePointDistancesProcess _calculateDistancesProcess;
        private readonly StartPortalsList _startPortals = new();
        private readonly EndPortalsList _endPortals = new();
        private readonly List<HexExitOption> _hexPortalsList = new();

        private readonly Dictionary<int, PortalNode> _nodes = new();
        private readonly HashSet<int> _activeNodeIds = new();

        private bool _isDisposeAvailable = true;
        private NativeList<int> _resultingPath;
        

        private struct PortalOption
        {
            public int PortalId;
            public int ZoneIndex;
            public float MinDist;
        }

        private struct PortalNode
        {
            public readonly int PortalId;
            public readonly float HeuristicValue;
            public float TotalPathCost => HeuristicValue + IntegrationValue;

            public float IntegrationValue;
            public int PreviousPortalId;
            public int StepsCount;

            public PortalNode(int portalId, float heuristic, float integration)
            {
                HeuristicValue = heuristic;
                PortalId = portalId;
                IntegrationValue = integration;

                PreviousPortalId = -1;
                StepsCount = 0;                
            }
        }

        public PortalsPathConstructionProcess(
            Allocator allocator, 
            INavigationMap map,
            IHexPortalsCoordinator portalsCoordinator,
            IPortalsLogic portalLogic)
        {
            _map = map;
            _portalsCoordinator = portalsCoordinator;
            _calculateDistancesProcess = new(allocator, _map);
            _pathsBuffer = _portalsCoordinator.GetPathsList();
            _portalLogic = portalLogic;

            _resultingPath = new(allocator);
        }

        protected override void DisposeResources()
        {
#if UNITY_EDITOR
            if (EditorPlaymodeLifetimeObject.IsQuitting)
            {
                try
                {
                    FinalDispose();
                }
                catch
                {
                    // ignore it
                }
                finally
                {
                }
            }
                return;
#else
            FinalDispose();
#endif
        }

        private void FinalDispose()
        {
            _calculateDistancesProcess.Dispose();
            _resultingPath.Dispose();
        }

        protected override async Awaitable ExecuteAsync(PortalConstructionProcessInput input)
        {
            var request = input.Request;
            await PreparePortalOptions(request.StartHexZoneIndex, request.StartHexCoord, request.EndHexCoord, request.StartTripos, _startPortals);
            await PreparePortalOptions(request.EndHexZoneIndex, request.EndHexCoord, request.StartHexCoord, request.EndTripos, _endPortals);

            if (_startPortals.Count == 0)
                throw new System.NotImplementedException($"start hex {input.Request.StartHexCoord} has no portals");

            if (_endPortals.Count == 0)
                throw new System.NotImplementedException($"end hex {input.Request.EndHexCoord} has no portals");

            //  sort start portals from closest to farthest
            _startPortals.Sort((optionA, optionB) => optionA.MinDist.CompareTo(optionB.MinDist));

#if ZE_NAVIGATION_DEBUG
            if (NavigationLogger.Settings.HasFlag(NavigationLogEvents.FullPortalSelectionLog))
                DEBUG_LogPortalOptions(input.Request);
 #endif

            var pathCost = PreparePortalsPath(input.Request.StartTripos);
            _pathsBuffer.AddCalculatedPath(input.ReservedPathId, FormResult(request, pathCost));

            _nodes.Clear();
            _activeNodeIds.Clear();
        }


        private float PreparePortalsPath(IntTriangularPos target)
        {
            // prepare initial nodes
            for (var i = 0; i < _startPortals.Count; i++)
            {
                var startPortal = _startPortals[i];

                var nodeData = new PortalNode(startPortal.PortalId, heuristic: CalculatePortalHeuristics(startPortal.PortalId, target), integration: startPortal.MinDist);

                _nodes.Add(nodeData.PortalId, nodeData);
                _activeNodeIds.Add(nodeData.PortalId);
            }

            // prepare for shortest path search
            var shortestPathOption = ShortestPathOption.Default;

            // handle all accessible nodes:
            do
            {
                var nextNode = GetNextNode();
                if (_endPortals.ContainsKey(nextNode.PortalId))
                    shortestPathOption = shortestPathOption.TryUpdate(nextNode.PortalId, nextNode.TotalPathCost);
                else 
                    HandleConnectedPortals(nextNode, target);
            }
            while (_activeNodeIds.Count != 0);


            // select shortest path:
            
            foreach (var endPortalOption in _endPortals.Values)
            {
                var endPortalId = endPortalOption.PortalId;
                if (!_nodes.TryGetValue(endPortalId, out var endPortalNode))
                    continue;

                var pathCost = endPortalNode.TotalPathCost + endPortalOption.MinDist;
                shortestPathOption = shortestPathOption.TryUpdate(endPortalId, pathCost);
            }

            if (!shortestPathOption.IsValid)
                throw new System.NotImplementedException("shortest path not found");

            // fulfill resulting path
            var observingNode = _nodes[shortestPathOption.PortalId];
            var resultingPathCost = observingNode.TotalPathCost;
            _resultingPath.Clear();
            _resultingPath.InsertRange(0, observingNode.StepsCount + 1);

#if ZE_NAVIGATION_DEBUG
            if (NavigationLogger.Settings.HasFlag(NavigationLogEvents.PortalsPathBestResult))
                UnityEngine.Debug.Log($"shortest path final node: {shortestPathOption.PortalId}, length: {observingNode.StepsCount}, prev: {observingNode.PreviousPortalId}");
#endif

            for (var i = observingNode.StepsCount; i > 0; i--)
            {
                _resultingPath[i] = observingNode.PortalId;
                observingNode = _nodes[observingNode.PreviousPortalId];
            }

            _resultingPath[0] = observingNode.PortalId;

            return resultingPathCost;
        }

        private async Awaitable PreparePortalOptions(
           int startZone,
           int2 hexCoord,
           int2 targetHexCoord,
           IntTriangularPos pos,
           IPortalsList portalOptions)
        {
            _isDisposeAvailable = false;
            // 1. calculate distances map through job
            _calculateDistancesProcess.Launch(new(0, hexCoord, pos));
            do
            {
                await Awaitable.NextFrameAsync();
            }
            while (_calculateDistancesProcess.Stage == CalculationProcessStage.Calculating);
            _isDisposeAvailable = true;

            if (StopProcessRequired)
                return;


            // 2. get all accessible portals, write also shortest distance
            portalOptions.Clear();
            var directionCoefficients = HexTransitionLogic.GetDirectionCostCoefficients(hexCoord, targetHexCoord);

            _hexPortalsList.Clear();
            _portalsCoordinator.GetHexPortalExits(startZone, hexCoord, _hexPortalsList);
            var calculationDistancesResult = _calculateDistancesProcess.StopAndGetResults();
            foreach (var exitOption in _hexPortalsList)
            {
                var exitData = exitOption.ExitData;
                var edge = exitData.Edge;
                
                var minDist = float.MaxValue;
                var portalCf = directionCoefficients[edge];

                foreach (var portalTriangle in edge.GetEdgeEnumerable(exitData))
                {
                    minDist = math.min(minDist, calculationDistancesResult.GetDistance(portalTriangle) * portalCf);                    
                }

            if (minDist == float.MaxValue)
                    continue;

                portalOptions.Add(new() { MinDist = minDist, PortalId = exitOption.PortalId, ZoneIndex = exitData.ZoneIndex });
            }
        }


        private PortalNode GetNextNode()
        {
            var minDist = float.MaxValue;
            PortalNode nextNode = default;

            foreach (var nodeId in _activeNodeIds)
            {
                var nodeData = _nodes[nodeId];
                if (nodeData.TotalPathCost < minDist)
                {
                    minDist = nodeData.TotalPathCost;
                    nextNode = nodeData;
                }
            }

            _activeNodeIds.Remove(nextNode.PortalId);
            return nextNode;
        }


        // update selected node neighbours and add untouched ones into active nodes list
        private void HandleConnectedPortals(PortalNode currentNode, IntTriangularPos target)
        {
            if (!_portalsCoordinator.TryGetPortalConnections(currentNode.PortalId, out var connections))
                return;

            foreach (var connection in connections)
            {
                var connectedPortalId = connection.Key;
                var transitionCost = connection.Value;
                var currentNodeNextPathCost = transitionCost + currentNode.IntegrationValue;


                if (_nodes.TryGetValue(connectedPortalId, out var connectedNode))
                {
                    if (connectedNode.IntegrationValue > currentNodeNextPathCost)
                    {
                        connectedNode.IntegrationValue = currentNode.IntegrationValue + transitionCost;
                        connectedNode.PreviousPortalId = currentNode.PortalId;
                        connectedNode.StepsCount = currentNode.StepsCount + 1;

                        _nodes[currentNode.PortalId] = connectedNode;
                    }
                }
                else
                {
                    var newNode = new PortalNode(connectedPortalId, integration: currentNodeNextPathCost, heuristic: CalculatePortalHeuristics(connectedPortalId, target));
                    newNode.PreviousPortalId = currentNode.PortalId;
                    newNode.StepsCount = currentNode.StepsCount + 1;
                    _activeNodeIds.Add(connectedPortalId);
                    _nodes.Add(connectedPortalId, newNode);
                }
            }
        }


       

        private float CalculatePortalHeuristics(int portalId, IntTriangularPos targetTripos)
        {
            var portalCenter = _portalLogic.GetPortalCenterTriangular(portalId);
            return TriangularMath.CalculateTriangularDistance(portalCenter, targetTripos.ToFloat3());
        }

        private PathCalculationResult<PortalPathDestinationKey, int> FormResult(in HexPathSearchRequest request, float pathCost)
        {
            var startKey = new PortalPathDestinationKey(request.StartHexCoord, request.StartHexZoneIndex);
            var endKey = new PortalPathDestinationKey(request.EndHexCoord, request.EndHexZoneIndex);
            return new PathCalculationResult<PortalPathDestinationKey, int>(
                start: startKey,
                end: endKey,
                readOnlyPoints: _resultingPath.AsArray().AsReadOnly(),
                pathCost: pathCost,
                hasReachedTarget: true );
        }

        #if ZE_NAVIGATION_DEBUG
        private void DEBUG_LogPortalOptions(in HexPathSearchRequest request)
        {
            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.AppendLine($"{request.StartHexCoord} zone {request.StartHexZoneIndex} -> {request.EndHexCoord} zone {request.EndHexZoneIndex}");
            stringBuilder.AppendLine("start portals:");
            foreach (var startPortal in _startPortals)
            {
                stringBuilder.AppendLine($"{startPortal.PortalId} : {startPortal.MinDist}");
            }

            stringBuilder.AppendLine("end portals: ");
            foreach (var endPortal in _endPortals.Values)
            {
                stringBuilder.AppendLine($"{endPortal.PortalId} : {endPortal.MinDist}");
            }

            UnityEngine.Debug.Log(stringBuilder.ToString());
        }
        #endif
    }
}
