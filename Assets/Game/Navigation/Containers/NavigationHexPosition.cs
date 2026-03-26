using System;
using System.Collections.Generic;
using Unity.Mathematics;


namespace ZE.MechBattle.Navigation
{
    public readonly struct NavigationHexPosition
    {
        public readonly int2 HexCoordinate;
        public readonly float2 CenterPosWorld;
        public readonly IntTriangularPos InnerRingTopTriangle;
        public float3 CenterPos3DWorld => new float3(CenterPosWorld.x, 0f, CenterPosWorld.y);
        public IntTriangularPos TriangularCenterPos => new IntTriangularPos(InnerRingTopTriangle.X, InnerRingTopTriangle.Y - 1, InnerRingTopTriangle.Z);

        public NavigationHexPosition(int hexCoordX, int hexCoordY, float hexEdge, float triangleHeight)
        {
            HexCoordinate = new(hexCoordX, hexCoordY);
            CenterPosWorld = HexMath.HexToWorld(HexCoordinate, hexEdge);
            InnerRingTopTriangle = NavigationMapHelper.GetInnerCircleTopTriangle(CenterPosWorld, triangleHeight);

            // TODO: replace to Triangular center only!
        }
    }
}
