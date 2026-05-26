using Unity.Collections;

namespace ZE.MechBattle
{
    public struct PathCalculationResult<DestinationKey, NodeKey> 
        where NodeKey  : unmanaged 
        where DestinationKey: unmanaged
    {
        public readonly bool HasReachedTarget;
        public readonly DestinationKey Start;
        public readonly DestinationKey End;
        public readonly NativeArray<NodeKey>.ReadOnly Points;
        public readonly float PathCost;

        public PathCalculationResult(
            DestinationKey start,
            DestinationKey end,
            in NativeArray<NodeKey>.ReadOnly readOnlyPoints, 
            float pathCost, 
            bool hasReachedTarget)
        {
            Start = start;
            End = end;
            HasReachedTarget = hasReachedTarget;
            Points = readOnlyPoints;
            PathCost = pathCost;
        }
    }
}
