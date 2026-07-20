using Unity.Collections;
using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public interface IRaycastDataSource
    {
        static int GetArrayLength(MapSettings mapSettings) => mapSettings.RaycastsPerHex;
        void CopyRaycastDataInto(NativeArray<RaycastHit> receiver);
    
    }
}
