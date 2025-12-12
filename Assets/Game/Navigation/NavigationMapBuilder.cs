using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class NavigationMapBuilder
    {
        public static NavigatonMap Build(float2 bottomLeftCorner, float2 topRightCorner, float hexEdgeLength, int longestTrianglesRow)
        {
            var width = topRightCorner.x - bottomLeftCorner.x;
            var length = topRightCorner.y - bottomLeftCorner.y;
            var center = new float2(bottomLeftCorner.x + width * 0.5f, bottomLeftCorner.y + length * 0.5f);           

            var hexHeight = hexEdgeLength * math.sqrt(3f) * 0.5f;

            // note: x-axis here is not (1,0,0), that why so complicated calculation
            // let the outer hex circle be with radius R (hexEdgeLength) and the inner will be r
            // let all distance from center to right side be d (width * 0.5f)
            // continue some r until intersection with right side, the hypotenuse will be r + x
            // then, by similarity principle we can found x = (R * d) /r - r
            // x is actually 2 * r * n, n is unknown - it is hexes count in secondary orth (first one is vector2.up)
            var x = (hexEdgeLength * width * 0.5f) / hexHeight - hexHeight;
            var halfWidth = (int)math.ceil(x / (2f * hexHeight));
            var widthInHex = 2 * halfWidth + 1; // 2 sides + 1 hex in center

            var halfHeight = (int)math.ceil((length * 0.5f - hexHeight) / (2f * hexHeight));
            var lengthInHex = halfHeight * 2;

            var map = new NavigatonMap(new(center.x, 0f, center.y), widthInHex, lengthInHex, hexEdgeLength);

            // zero column is always odd (odd cells count)
            map.AddHex(new(0, 0));
            for (var j = 1; j <= halfHeight; j++)
            {
                map.AddHex(new(0, j));
                map.AddHex(new(0, -j));
            }

            // columns count also odd
            for (var i = 1; i <= halfWidth; i++)
            {
                for (var j = -halfHeight + (i+1)/2; j <= halfHeight + i/2; j++)
                {
                    map.AddHex(new(i, j));
                    map.AddHex(new(-i, j-i));
                }
            }          

            return map;
        }

        //private void DoTriangles()
        //{
        //    var trianglesWidth = math.round(width / triangleEdgeLength);
        //    var excessTriangles = trianglesWidth % 4;
        //    if (excessTriangles != 0)
        //    {
        //        var extend = excessTriangles * 0.5f * triangleEdgeLength;
        //        bottomLeftCorner.x -= extend;
        //        topRightCorner.x += extend;
        //        trianglesWidth += (4 - excessTriangles);
        //    }

        //    var triangleHeight = triangleEdgeLength * math.sqrt(3) * 0.5f;
        //    var trianglesLength = math.round(length / triangleHeight);
        //    excessTriangles = trianglesLength % 4;
        //    if (excessTriangles != 0)
        //    {
        //        var extend = excessTriangles * 0.5f * triangleHeight;
        //        bottomLeftCorner.y -= extend;
        //        topRightCorner.y += extend;
        //        trianglesLength += (4 - excessTriangles);
        //    }

        //    var trisEdgeLength = math.max(trianglesWidth, triangleEdgeLength);
        //   
        //}
    
    }
}
