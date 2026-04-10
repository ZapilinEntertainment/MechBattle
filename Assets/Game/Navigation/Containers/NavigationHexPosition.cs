using System;
using System.Collections.Generic;
using Unity.Mathematics;


namespace ZE.MechBattle.Navigation
{
    public readonly struct NavigationHexPosition
    {
        public readonly int2 HexCoordinate;
        public readonly float2 CenterPosWorld;
        public readonly IntTriangularPos InnerRingTopValleyTriangle;
        public float3 CenterPos3DWorld => new float3(CenterPosWorld.x, 0f, CenterPosWorld.y);
        public IntTriangularPos TriangularCenterPos => new IntTriangularPos(InnerRingTopValleyTriangle.X, InnerRingTopValleyTriangle.Y - 1, InnerRingTopValleyTriangle.Z);

        public NavigationHexPosition(int hexCoordX, int hexCoordY, float hexEdge, float triangleHeight) : 
            this(hexCoord: new(hexCoordX, hexCoordY), hexEdge: hexEdge, triangleHeight: triangleHeight)
        { }

        public NavigationHexPosition(int2 hexCoord, float hexEdge, int trianglesPerEdge) : 
            this(hexCoord: hexCoord, hexEdge: hexEdge, triangleHeight: hexEdge / trianglesPerEdge * NavigationConstants.SQRT_OF_THREE_HALVED) 
        { }

        public NavigationHexPosition(IntTriangularPos anyHexTriangle, INavigationMap map) :
            this(hexCoord: TriangularMath.TriangularToHex(anyHexTriangle, map.TriangleHeight, map.HexEdgeSize),
                hexEdge: map.HexEdgeSize,
                triangleHeight: map.TriangleHeight)
        { }

        public NavigationHexPosition(HexPathNodeKey hexNode, INavigationMap map) :
            this(hexCoord: hexNode.HexCoord,
                hexEdge: map.HexEdgeSize,
                triangleHeight: map.TriangleHeight)
        { }
        public NavigationHexPosition(int2 hexCoord, INavigationMap map) :
           this(hexCoord: hexCoord,
               hexEdge: map.HexEdgeSize,
               triangleHeight: map.TriangleHeight)
        { }

        private NavigationHexPosition(int2 hexCoord, float hexEdge, float triangleHeight)
        {
            HexCoordinate = hexCoord;
            CenterPosWorld = HexMath.HexToWorld(HexCoordinate, hexEdge);
            InnerRingTopValleyTriangle = NavigationMapHelper.GetInnerCircleTopTriangle(CenterPosWorld, triangleHeight);
        }
    }
}
