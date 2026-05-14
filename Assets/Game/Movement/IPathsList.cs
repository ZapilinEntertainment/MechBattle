using UnityEngine;

namespace ZE.MechBattle
{
    public interface IPathsList<NodeKey> where NodeKey : unmanaged
    {
        void AddCalculatedPath(int pathKey, PathCalculationResult<NodeKey> calculatedData);
        PathData<NodeKey> ReservePath((NodeKey, NodeKey) destinationKey);
    
    }
}
