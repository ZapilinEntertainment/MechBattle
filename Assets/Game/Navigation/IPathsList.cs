using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public interface IPathsList<DestinationKey, NodeKey>
        where NodeKey : unmanaged
        where DestinationKey : unmanaged
    {
        void AddCalculatedPath(int pathKey, PathCalculationResult<DestinationKey, NodeKey> calculatedData);
        int ReservePath(DestinationKey start, DestinationKey end);
    }
}
