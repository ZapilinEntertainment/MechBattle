using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class GetHexesInRectangleCommand
    {
        public static List<int2> Execute(
            float2 worldMin,
            float2 worldMax,
            float edge)
        {
            var result = new List<int2>();

            var bottomCorner = TriangularMath.WorldToHex(worldMin, edge);
            var topCorner = TriangularMath.WorldToHex(worldMax, edge);
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
