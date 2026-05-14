using UnityEngine;
using Unity.Collections;

namespace ZE.MechBattle
{
    public class PathData<NodeKey> where NodeKey : unmanaged
    {
        public readonly int Id;
        public readonly (NodeKey, NodeKey) DestinationKey;

        public bool IsCalculated { get; private set; }
        public bool HasReachedTarget { get; private set; }
        public float PathCost { get; private set; }
        public NodeKey[] Points { get; private set; }        

        public int NodesCount => Points.Length;
        public float LastUseTime { get; private set; }
        public NodeKey LastNode => NodesCount != 0 ? Points[NodesCount-1] : default;

        public PathData(int id, (NodeKey, NodeKey) destinationKey)
        {
            Id = id;
            DestinationKey = destinationKey;
            IsCalculated = false;
        }

        public void OnCalculationFinished(PathCalculationResult<NodeKey> calculationResult)
        {
            IsCalculated = true;
            Points = calculationResult.Points.ToArray();
            PathCost = calculationResult.PathCost;
            HasReachedTarget = calculationResult.HasReachedTarget;
            LastUseTime = Time.time;            
        }

        public bool TryGetTriangle(int stepIndex, out NodeKey pos)
        {
            if (stepIndex < 0 || stepIndex >= NodesCount)
            {
                pos = default;
                return false;
            }

            pos = Points[stepIndex];
            return true;
        }

        public void UpdateUseTime() => LastUseTime = Time.time;

        public bool TryGetNode(int index, out NodeKey node)
        {
            if (index < 0 || index >= NodesCount)
            {
                node = default;
                return false;
            }

            node = Points[index];
            return true;
        }
    }
}
