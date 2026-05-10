using UnityEngine;
using Unity.Collections;

namespace ZE.MechBattle
{
    public class PathData<NodeKey> where NodeKey : unmanaged
    {
        public readonly NodeKey[] Points;
        public int Length => Points.Length;
        public float LastUseTime { get; private set; }

        public (NodeKey, NodeKey) GetDestinationKey() => new(Points[0], Points[Length - 1]);

        public PathData(in NativeArray<NodeKey> readList)
        {
            Points = readList.ToArray();
            LastUseTime = Time.time;
        }

        public bool TryGetTriangle(int stepIndex, out NodeKey pos)
        {
            if (stepIndex < 0 || stepIndex >= Length)
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
            if (index < 0 || index >= Length)
            {
                node = default;
                return false;
            }

            node = Points[index];
            return true;
        }
    }
}
