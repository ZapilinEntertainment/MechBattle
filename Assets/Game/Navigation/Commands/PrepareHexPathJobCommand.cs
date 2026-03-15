using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class PrepareHexPathJobCommand
    {
        public static ConstructHexPathJob Execute(float3 startPos, float3 endPos, NavigationMap map)
        {
            var startHex = HexMath.WorldToHex(startPos.xz, map.HexEdgeSize);
            var endHex = HexMath.WorldToHex(endPos.xz, map.HexEdgeSize);

            // todo: get nearest edges
        }
    
    }
}
