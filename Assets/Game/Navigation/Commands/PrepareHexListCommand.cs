using UnityEngine;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public static class PrepareHexListCommand
    {
        public static int Execute(NavigationMap map)
        {
            var edge = map.HexEdgeSize;
            var trianglesPerEdge = map.TrianglesPerHexEdge;
            var triangleEdgeSize = edge / trianglesPerEdge;
            var trianglesInHexCount = TriangularMath.GetTrianglesCountInHex(trianglesPerEdge);

            var hexEdgeSize = map.HexEdgeSize;
            var settings = map.Settings;
            using var hexList = GetHexesInRectangleCommand.Execute(settings.BottomLeftCorner, settings.TopRightCorner, hexEdgeSize, triangleEdgeSize, Allocator.Temp);
            foreach (var hex in hexList)
            {
                map.AddHex(hex);
            }

            return hexList.Length;
        }
    }
}
