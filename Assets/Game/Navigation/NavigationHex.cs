using System;
using System.Collections.Generic;
using Unity.Mathematics;


namespace ZE.MechBattle.Navigation
{
    public enum HexEdge : byte { Up, UpRight, DownRight,Down, DownLeft, UpLeft }

    public readonly struct NavigationHex
    {
        public readonly int2 HexCoordinate;
        public readonly float2 CenterPos;
        public readonly IntTriangularPos InnerRingTopTriangle;
        public float3 CenterPos3D => new float3(CenterPos.x, 0f, CenterPos.y);

        public NavigationHex(int hexCoordX, int hexCoordY, float hexEdge, float triangleEdge)
        {
            HexCoordinate = new(hexCoordX, hexCoordY);
            CenterPos = TriangularMath.HexToWorld(HexCoordinate, hexEdge);
            InnerRingTopTriangle = NavigationMapHelper.GetInnerCircleTopTriangle(CenterPos, triangleEdge);
        }
    }
}
