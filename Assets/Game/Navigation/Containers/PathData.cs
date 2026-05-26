using UnityEngine;
using Unity.Collections;
using ZE.Utils;

namespace ZE.MechBattle.Navigation
{
    public class PathData<DestinationKey, NodeKey> : ILRUBufferElement
        where NodeKey : unmanaged
        where DestinationKey : unmanaged
    {
        public readonly int Id;
        public readonly (DestinationKey start, DestinationKey end) DestinationKeys;

        public bool IsCalculated { get; private set; }
        public bool HasReachedTarget { get; private set; }
        public float PathCost { get; private set; }
        public NodeKey[] Points { get; private set; }        

        public int NodesCount => Points.Length;
        public float LastUseTime { get; private set; }
        public NodeKey LastNode => NodesCount != 0 ? Points[NodesCount-1] : default;

        public PathData(int id, (DestinationKey, DestinationKey) destinationKey)
        {
            Id = id;
            DestinationKeys = destinationKey;
            IsCalculated = false;
        }

        public void TrimPath(int lastStepIndex)
        {
            var newPoints = new NodeKey[lastStepIndex +1];
            for (var i = 0; i < newPoints.Length ; i++)
            {
                newPoints[i] = Points[i];
            }

            Points = newPoints;
            HasReachedTarget = false;
            LastUseTime = Time.time;
        }

        public void OnCalculationFinished(PathCalculationResult<DestinationKey, NodeKey> calculationResult)
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
