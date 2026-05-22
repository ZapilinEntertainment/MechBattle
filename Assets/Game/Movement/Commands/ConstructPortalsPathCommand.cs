using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public static class ConstructPortalsPathCommand
    {
        private struct PortalOption
        {
            public int PortalId;
            public int ZoneIndex;
            public float MinDist;
        }

        public static async Awaitable<int[]> Execute(
            HexPathSearchRequest request,
            INavigationMap map,
            PortalConnectionsList portalsConnectionsList,
            CancellationToken cancellationToken)
        {
            var startPortals = await PreparePortalOptions(request.StartHexCoord, request.EndHexCoord, request.StartTripos, map, cancellationToken);
            var endPortals = await PreparePortalOptions(request.EndHexCoord, request.StartHexCoord, request.EndTripos, map, cancellationToken);

            
            if (startPortals.Count == 0)
                throw new System.NotImplementedException("start hex has no portals");

            if (endPortals.Count == 0)
                throw new System.NotImplementedException("end hex has no portals");

            return PreparePortalsPath(startPortals, endPortals, portalsConnectionsList);
        }

        private static async Awaitable<List<PortalOption>> PreparePortalOptions(
            int2 hexCoord, 
            int2 targetHexCoord, 
            IntTriangularPos pos, 
            INavigationMap map,
            CancellationToken cancellationToken)
        {
            using var distanceCalculationProcess = new GeneratePointDistancesProcess(Allocator.TempJob, map);
            var distanceCalculationHandle = distanceCalculationProcess.Schedule(hexCoord, pos);

            do
            {
                await Awaitable.NextFrameAsync();
            }
            while (!distanceCalculationHandle.IsCompleted);
            if (cancellationToken.IsCancellationRequested)
                return default;

            var distancesData = new Dictionary<IntTriangularPos, float>();
            distanceCalculationProcess.UnloadDistanceDataInto(distancesData);


            //  get all accessible portals, write also shortest distance
            var portalsList = new List<PortalOption>();
            var directionCoefficients = HexTransitionLogic.GetDirectionCostCoefficients(hexCoord, targetHexCoord);

            var hex = map.GetOrCreateHex(hexCoord);
            foreach (var portal in hex.PortalsList)
            {
                var portalExit = portal.GetExit(hexCoord);
                var enumerator = portalExit.Edge.GetEdgeEnumerable(portalExit.Length, portalExit.StartTriangle);

                var minDist = float.MaxValue;
                var portalCf = directionCoefficients[portalExit.Edge];
                foreach (var portalTriangle in enumerator)
                {
                    minDist = math.min(minDist, distancesData[portalTriangle] * portalCf);
                }
                if (minDist == float.MaxValue)
                    continue;

                portalsList.Add(new() { MinDist = minDist, PortalId = portal.Id, ZoneIndex = portalExit.ZoneIndex });
            }

            //  sort portals from closest to farthest, (reversed)
            portalsList.Sort((optionA, optionB) => optionB.MinDist.CompareTo(optionA.MinDist));
            return portalsList;
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

        private static int[] PreparePortalsPath(
            List<PortalOption> startPortalsList,
            List<PortalOption> endPortalsList,
            PortalConnectionsList portalConnectionsList)
        {            
            var nodes = new Dictionary<int, PortalNode>();
            var activeNodeIds = new HashSet<int>();  

            // prepare initial nodes
            for (var i = 0; i < startPortalsList.Count; i++)
            {
                var startPortal = startPortalsList[i];
                
                var nodeData = new PortalNode( startPortal.PortalId, startPortal.MinDist);

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

            void HandleConnectedPortals(PortalNode node)
            {
                if (!portalConnectionsList.TryGetPortalConnections(node.PortalId, out var connections))
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

            do
            {
                var nextNode = GetNextNode();
                HandleConnectedPortals(nextNode);
            }
            while (activeNodeIds.Count != 0);   
            
            // select shortest path
            var shortestPathLength = float.MaxValue;
            var shortestPathEndPortalId = -1;
            foreach (var endPortalOption in endPortalsList)
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
                throw new System.Exception("shortest path not found");

            // build path from end to start
            var observingNode = nodes[shortestPathEndPortalId];
            var path = new int[observingNode.StepsCount];

            for (var i = observingNode.StepsCount - 1; i >= 0; i--)
            {
                path[i] = observingNode.PortalId;
                observingNode = nodes[observingNode.PreviousPortalId];
            }

            return path;
        }

    }
}
