using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.PortalPathCalculation
{
    internal static class CalculateShortestPortalsPathCommand
    {           
        internal static float Execute(IPortalsPathCalculationObject protocol, int2 startHexCoord, IntTriangularPos targetTripos)
        {
            PrepareInitialNodes(protocol, targetTripos);
            var shortestPathOption = HandleAllNodes(protocol, targetTripos);
            SelectClosestExit(protocol, ref shortestPathOption);

            if (!shortestPathOption.IsValid) 
            {
                UnityEngine.Debug.LogError("shortest path not found");
                return 0f;
            }

            return FulfilResultingPath(protocol, targetTripos, startHexCoord, shortestPathOption);
        }

        private static void PrepareInitialNodes(IPortalsPathCalculationObject protocol, IntTriangularPos targetTripos)
        {
            for (var i = 0; i < protocol.StartPortals.Count; i++)
            {
                var startPortal = protocol.StartPortals[i];

                var nodeData = new PortalNode(
                    startPortal.PortalId,
                    heuristic: CalculatePortalHeuristics(startPortal.PortalId, targetTripos, protocol.PortalsHandler),
                    integration: startPortal.MinDist);

                protocol.Nodes.Add(nodeData.PortalId, nodeData);
                protocol.ActiveNodeIds.Add(nodeData.PortalId);
            }
        }

        private static ShortestPathOption HandleAllNodes(IPortalsPathCalculationObject protocol, IntTriangularPos targetTripos)
        {
            // prepare for shortest path search
            var shortestPathOption = ShortestPathOption.Default;

            // handle all accessible nodes:
            do
            {
                var nextNode = protocol.GetNextNode();
                if (protocol.EndPortals.ContainsKey(nextNode.PortalId))
                    shortestPathOption = shortestPathOption.TryUpdate(nextNode.PortalId, nextNode.TotalPathCost);
                else
                    HandleConnectedPortals(protocol, nextNode, targetTripos);
            }
            while (protocol.ActiveNodeIds.Count != 0);
            return shortestPathOption;
        }


        // update selected node neighbours and add untouched ones into active nodes list
        private static void HandleConnectedPortals(IPortalsPathCalculationObject protocol, PortalNode currentNode, IntTriangularPos target)
        {
            if (!protocol.PortalsCoordinator.TryGetPortalConnections(currentNode.PortalId, out var connections))
                return;

            foreach (var connection in connections)
            {
                var connectedPortalId = connection.Key;
                var transitionCost = connection.Value;
                var currentNodeNextPathCost = transitionCost + currentNode.IntegrationValue;


                if (protocol.Nodes.TryGetValue(connectedPortalId, out var connectedNode))
                {
                    if (connectedNode.IntegrationValue > currentNodeNextPathCost)
                    {
                        connectedNode.IntegrationValue = currentNode.IntegrationValue + transitionCost;
                        connectedNode.PreviousPortalId = currentNode.PortalId;
                        connectedNode.StepsCount = currentNode.StepsCount + 1;

                        protocol.Nodes[connectedPortalId] = connectedNode;
                        protocol.ActiveNodeIds.Add(connectedPortalId);
                    }
                }
                else
                {
                    var newNode = new PortalNode(
                        connectedPortalId,
                        integration: currentNodeNextPathCost,
                        heuristic: CalculatePortalHeuristics(connectedPortalId, target, protocol.PortalsHandler));
                    newNode.PreviousPortalId = currentNode.PortalId;
                    newNode.StepsCount = currentNode.StepsCount + 1;
                    protocol.ActiveNodeIds.Add(connectedPortalId);
                    protocol.Nodes.Add(connectedPortalId, newNode);
                }
            }
        }

        private static void SelectClosestExit(IPortalsPathCalculationObject protocol, ref ShortestPathOption shortestPathOption)
        {
            // handle exits and try get best path
            foreach (var endPortalOption in protocol.EndPortals.Values)
            {
                var endPortalId = endPortalOption.PortalId;
                if (!protocol.Nodes.TryGetValue(endPortalId, out var endPortalNode))
                    continue;

                var pathCost = endPortalNode.IntegrationValue + endPortalOption.MinDist;
                shortestPathOption = shortestPathOption.TryUpdate(endPortalId, pathCost);
            }
        }

        private static float FulfilResultingPath(IPortalsPathCalculationObject protocol, IntTriangularPos target, int2 startHexCoord, ShortestPathOption shortestPathOption)
        {
            var observingNode = protocol.Nodes[shortestPathOption.PortalId];
            var resultingPathCost = shortestPathOption.Length;

            var resultingPath = protocol.ResultingPath;
            resultingPath.Clear();

            var length = observingNode.StepsCount + 1;
            resultingPath.InsertRange(0, length);

#if ZE_NAVIGATION_DEBUG
            if (NavigationLogger.Settings.HasFlag(NavigationLogEvents.PortalsPathBestResult))
                UnityEngine.Debug.Log($"shortest path final node: {shortestPathOption.PortalId}, length: {length}, prev: {observingNode.PreviousPortalId}");
#endif

            for (var i = length-1; i > 0; i--)
            {
                resultingPath[i] = observingNode.PortalId;
                observingNode = protocol.Nodes[observingNode.PreviousPortalId];
            }

            resultingPath[0] = observingNode.PortalId;

            protocol.ResultingPath = FilterPortalsPathCommand.Execute(protocol.PortalsCoordinator, resultingPath, startHexCoord);
            return resultingPathCost;
        }

        private static float CalculatePortalHeuristics(int portalId, IntTriangularPos targetTripos, IPortalsHandler logic)
        {
            var portalCenter = logic.GetPortalCenterTriangular(portalId);
            return TriangularMath.CalculateTriangularDistance(portalCenter, targetTripos.ToFloat3());
        }
    }
}
