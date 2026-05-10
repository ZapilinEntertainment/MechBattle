using UnityEngine;
using Unity.Collections;

namespace ZE.MechBattle
{
    public class PathData<NodeKey> where NodeKey : unmanaged
    {
        public readonly float PathCost;
        public readonly NodeKey[] Points;
        public int NodesCount => Points.Length;
        public float LastUseTime { get; private set; }

        public (NodeKey, NodeKey) GetDestinationKey() => new(Points[0], Points[NodesCount - 1]);

        public PathData(NativeArray<NodeKey>.ReadOnly readList, float pathCost)
        {
            Points = readList.ToArray();
            PathCost = pathCost;
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
