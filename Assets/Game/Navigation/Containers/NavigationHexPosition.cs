using System;
using System.Collections.Generic;
using Unity.Mathematics;


namespace ZE.MechBattle.Navigation
{
    public readonly struct NavigationHexPosition
    {
        public readonly int2 HexCoordinate;
        public readonly float2 CenterPos;
        public readonly IntTriangularPos InnerRingTopTriangle;
        public float3 CenterPos3D => new float3(CenterPos.x, 0f, CenterPos.y);
        public IntTriangularPos TriangularCenterPos => new IntTriangularPos(InnerRingTopTriangle.X, InnerRingTopTriangle.Y - 1, InnerRingTopTriangle.Z);

        public NavigationHexPosition(int hexCoordX, int hexCoordY, float hexEdge, float triangleEdge)
        {
            HexCoordinate = new(hexCoordX, hexCoordY);
            CenterPos = TriangularMath.HexToWorld(HexCoordinate, hexEdge);
            InnerRingTopTriangle = NavigationMapHelper.GetInnerCircleTopTriangle(CenterPos, triangleEdge);
        }
    }
}
