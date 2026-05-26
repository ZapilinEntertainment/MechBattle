using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public interface IPathsList<DestinationKey, NodeKey> 
        where NodeKey : unmanaged
        where DestinationKey: unmanaged
    {
        PathData<DestinationKey,NodeKey> AddCalculatedPath(int pathKey, PathCalculationResult<DestinationKey, NodeKey> calculatedData);
        PathData<DestinationKey, NodeKey> ReservePath(DestinationKey start, DestinationKey end);
    }
}
