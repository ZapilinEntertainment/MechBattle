using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class DefineAccessibleHexEdgesCommand
    {
        public static HexEdgesMask Execute(float3 worldPos, NavigationMap map)
        {
            var hexCoord = HexMath.DefineHex(worldPos.xz, map.HexEdgeSize);
            var flowMap = map.GetFlowMap(hexCoord);
            var triangularPos = TriangularMath.WorldToTrianglePos(worldPos, map.TriangleEdgeSize);
            var cellData = flowMap.GetCombinedCellData(triangularPos);
            var edgesAccessMask = cellData.GetCombinedEdgeAccessMask();
            return new HexEdgesMask(edgesAccessMask);
        }
    }
}
