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
        protected override bool IsDisposeAvailable => _isDisposeAvailable;

        private readonly INavigationMap _map;
        private readonly IHexPortalsCoordinator _portalsCoordinator;
        private readonly IPathsList<PortalPathDestinationKey, int> _pathsBuffer;

        private readonly GeneratePointDistancesProcess _generatePointDistancesProcess;
        private readonly List<PortalOption> _startPortals = new();
        private readonly List<PortalOption> _endPortals = new();
        private readonly List<HexExitOption> _hexPortalsList = new();

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
            public readonly float HeuristicValue;
            public readonly int PortalId;

            public float PathCost => IntegrationValue + HeuristicValue;

            public float IntegrationValue;
            public int PreviousPortalId;
            public int StepsCount;

            public PortalNode(int portalId, float heuristicValue)
            {
                HeuristicValue = heuristicValue;
                PortalId = portalId;

                IntegrationValue = 0f;
                PreviousPortalId = -1;
                StepsCount = 0;
            }
        }

        public PortalsPathConstructionProcess(
            Allocator allocator, 
            INavigationMap map,
            IHexPortalsCoordinator portalsCoordinator)
        {
            _map = map;
            _portalsCoordinator = portalsCoordinator;
            _generatePointDistancesProcess = new(allocator, _map);
            _pathsBuffer = _portalsCoordinator.GetPathsList();

            _resultingPath = new(allocator);
        }

        protected override void DisposeResources()
        {
            _generatePointDistancesProcess.Dispose();
            _resultingPath.Dispose();
        }

        protected override async Awaitable ExecuteAsync(PortalConstructionProcessInput input)
        {
            var request = input.Request;
            await PreparePortalOptions(request.StartHexCoord, request.EndHexCoord, request.StartTripos, _startPortals);
            await PreparePortalOptions(request.EndHexCoord, request.StartHexCoord, request.EndTripos, _endPortals);

            if (_startPortals.Count == 0)
                throw new System.NotImplementedException("start hex has no portals");

            if (_endPortals.Count == 0)
                throw new System.NotImplementedException("end hex has no portals");

            var pathCost = PreparePortalsPath();
            _pathsBuffer.AddCalculatedPath(input.ReservedPathId, FormResult(request, pathCost));
        }

        private async Awaitable<List<PortalOption>> PreparePortalOptions(
           int2 hexCoord,
           int2 targetHexCoord,
           IntTriangularPos pos,
           List<PortalOption> portalOptions)
        {
            _isDisposeAvailable = false;
            // 1. calculate distances map through job
            var distanceCalculationHandle = _generatePointDistancesProcess.Schedule(hexCoord, pos);
            do
            {
                await Awaitable.NextFrameAsync();
            }
            while (!distanceCalculationHandle.IsCompleted);
            _isDisposeAvailable = true;

            if (StopProcessRequired)
                return default;


            // 2. get all accessible portals, write also shortest distance
            portalOptions.Clear();
            var directionCoefficients = HexTransitionLogic.GetDirectionCostCoefficients(hexCoord, targetHexCoord);

            _hexPortalsList.Clear();
            _portalsCoordinator.GetHexPortalExits(hexCoord, _hexPortalsList);
            foreach (var exitOption in _hexPortalsList)
            {
                var exitData = exitOption.ExitData;
                var edge = exitData.Edge;
                
                var minDist = float.MaxValue;
                var portalCf = directionCoefficients[edge];
                foreach (var portalTriangle in edge.GetEdgeEnumerable(exitData))
                {
                    minDist = math.min(minDist, _generatePointDistancesProcess.GetDistance(portalTriangle) * portalCf);
                }

            if (minDist == float.MaxValue)
                    continue;

                portalOptions.Add(new() { MinDist = minDist, PortalId = exitOption.PortalId, ZoneIndex = exitData.ZoneIndex });
            }

            //  sort portals from closest to farthest, (reversed)
            portalOptions.Sort((optionA, optionB) => optionB.MinDist.CompareTo(optionA.MinDist));
            return portalOptions;
        }

       

        private float PreparePortalsPath()
        {
            var nodes = new Dictionary<int, PortalNode>();
            var activeNodeIds = new HashSet<int>();

            // prepare initial nodes
            for (var i = 0; i < _startPortals.Count; i++)
            {
                var startPortal = _startPortals[i];

                var nodeData = new PortalNode(startPortal.PortalId, startPortal.MinDist);

                nodes.Add(nodeData.PortalId, nodeData);
                activeNodeIds.Add(nodeData.PortalId);
            }

            PortalNode GetNextNode()
            {
                var minDist = float.MaxValue;
                PortalNode nextNode = default;

                foreach (var nodeId in activeNodeIds)
                {
                    var nodeData = nodes[nodeId];
                    if (nodeData.PathCost < minDist)
                    {
                        minDist = nodeData.PathCost;
                        nextNode = nodeData;
                    }
                }

                return nextNode;
            }

            // update selected node neighbours and add untouched ones into active nodes list
            void HandleConnectedPortals(PortalNode node)
            {
                if (!_portalsCoordinator.TryGetPortalConnections(node.PortalId, out var connections))
                    return;

                foreach (var connection in connections)
                {
                    var portalId = connection.Key;
                    var transitionCost = connection.Value;

                    if (nodes.TryGetValue(portalId, out var connectedNode))
                    {
                        var newDistance = node.IntegrationValue + transitionCost;
                        if (connectedNode.IntegrationValue > transitionCost)
                        {
                            connectedNode.IntegrationValue = transitionCost;
                            connectedNode.PreviousPortalId = node.PortalId;
                            connectedNode.StepsCount = node.StepsCount + 1;

                            nodes[node.PortalId] = connectedNode;
                        }
                    }
                    else
                    {
                        var newNode = new PortalNode(portalId, node.HeuristicValue + transitionCost);
                        activeNodeIds.Add(newNode.PortalId);
                    }
                }
            }


            // handle all accessible nodes:
            do
            {
                var nextNode = GetNextNode();
                HandleConnectedPortals(nextNode);
            }
            while (activeNodeIds.Count != 0);


            // select shortest path:
            var shortestPathLength = float.MaxValue;
            var shortestPathEndPortalId = -1;
            foreach (var endPortalOption in _endPortals)
            {
                if (!nodes.TryGetValue(endPortalOption.PortalId, out var endPortalNode))
                    continue;

                var pathCost = endPortalNode.IntegrationValue + endPortalOption.MinDist;
                if (pathCost < shortestPathLength)
                {
                    shortestPathLength = pathCost;
                    shortestPathEndPortalId = endPortalNode.PortalId;
                }
            }

            if (shortestPathEndPortalId == -1)
                throw new System.NotImplementedException("shortest path not found");

            // fulfill resulting path
            var observingNode = nodes[shortestPathEndPortalId];
            var resultingPathCost = observingNode.PathCost;
            _resultingPath.Clear();
            _resultingPath.InsertRange(0, observingNode.StepsCount);

            for (var i = observingNode.StepsCount - 1; i >= 0; i--)
            {
                _resultingPath[i] = observingNode.PortalId;
                observingNode = nodes[observingNode.PreviousPortalId];
            }

            return resultingPathCost;
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
    }
}
