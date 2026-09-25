using System.Collections.Generic;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.PortalPathCalculation
{
    internal interface IPortalsPathCalculationObject
    {
        StartPortalsList StartPortals { get; }
        EndPortalsList EndPortals { get; }
        Dictionary<int, PortalNode> Nodes { get; }
        HashSet<int> ActiveNodeIds { get; }
        IPortalsHandler PortalsHandler { get; }
        IHexPortalsCoordinator PortalsCoordinator { get; }
        NativeList<int> ResultingPath { get; set; }

        public PortalNode GetNextNode()
        {
            var minDist = float.MaxValue;
            PortalNode nextNode = default;

            foreach (var nodeId in ActiveNodeIds)
            {
                var nodeData = Nodes[nodeId];
                if (nodeData.TotalPathCost < minDist)
                {
                    minDist = nodeData.TotalPathCost;
                    nextNode = nodeData;
                }
            }

            ActiveNodeIds.Remove(nextNode.PortalId);
            return nextNode;
        }
    }
}
