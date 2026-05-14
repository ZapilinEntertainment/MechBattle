using Unity.Collections;

namespace ZE.MechBattle
{
    public struct PathCalculationResult<NodeKey> where NodeKey : unmanaged 
    {
        public readonly bool HasReachedTarget;
        public readonly (NodeKey, NodeKey) RequestedDestination;
        public readonly NativeArray<NodeKey>.ReadOnly Points;
        public readonly float PathCost;

        public PathCalculationResult(
            (NodeKey, NodeKey) requestedDestination, 
            in NativeArray<NodeKey> points, 
            float pathCost, 
            bool hasReachedTarget)
        {
            RequestedDestination = requestedDestination;
            HasReachedTarget = hasReachedTarget;
            Points = points.AsReadOnly();
            PathCost = pathCost;
        }
    }
}
