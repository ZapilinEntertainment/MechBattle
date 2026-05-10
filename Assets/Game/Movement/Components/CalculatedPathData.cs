using Unity.Collections;

namespace ZE.MechBattle
{
    public struct CalculatedPathData<NodeKey> where NodeKey : unmanaged 
    {
        public readonly NativeArray<NodeKey>.ReadOnly Points;
        public readonly float PathCost;

        public CalculatedPathData(in NativeArray<NodeKey> points, float pathCost)
        {
            Points = points.AsReadOnly();
            PathCost = pathCost;
        }
    }
}
