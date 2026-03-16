using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public static class GetHexesInRectangleCommand
    {
        public static NativeList<int2> Execute(
            float2 worldMin,
            float2 worldMax,
            float hexEdge,
            float triangleEdge,
            Allocator allocator)
        {
            var result = new NativeList<int2>(allocator);

            var bottomCorner = HexMath.DefineHex(worldMin, hexEdge);
            var topCorner = HexMath.DefineHex(worldMax, hexEdge);
            var yOffset = (int)math.ceil(topCorner.x - bottomCorner.x / 2);

            var width = topCorner.x - bottomCorner.x + 1;
            for (var x = 0; x < width; x++)
            {
                int offset = x / 2;
                for (var y = bottomCorner.y - offset; y < topCorner.y + (yOffset - offset); y++)
                {
                    result.Add(new(x + bottomCorner.x, y));
                }
            }
            

            return result;
        }
    }
}
