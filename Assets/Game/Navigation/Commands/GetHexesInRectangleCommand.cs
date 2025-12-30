using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class GetHexesInRectangleCommand
    {
        public static List<NavigationHex> Execute(
            float2 worldMin,
            float2 worldMax,
            float hexEdge,
            float triangleEdge)
        {
            var result = new List<NavigationHex>();

            var bottomCorner = TriangularMath.WorldToHex(worldMin, hexEdge);
            var topCorner = TriangularMath.WorldToHex(worldMax, hexEdge);
            var yOffset = (int)math.ceil(topCorner.x - bottomCorner.x / 2);

            var width = topCorner.x - bottomCorner.x + 1;
            for (var x = 0; x < width; x++)
            {
                int offset = x / 2;
                for (var y = bottomCorner.y - offset; y < topCorner.y + (yOffset - offset); y++)
                {
                    result.Add(new(x + bottomCorner.x, y, hexEdge, triangleEdge));
                }
            }
            

            return result;
        }
    }
}
