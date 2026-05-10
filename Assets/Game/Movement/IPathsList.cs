using UnityEngine;

namespace ZE.MechBattle
{
    public interface IPathsList<NodeKey> where NodeKey : unmanaged
    {
        void AddCalculatedPath(int pathKey, CalculatedPathData<NodeKey> calculatedData);
        int ReservePathId();
    
    }
}
