using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public readonly struct TriangleDrawData
    {
        public readonly Vector3[] Vertices;
        public readonly bool IsPassable;
        private const float DRAW_HEIGHT_OFFSET = 0.01f;

        public TriangleDrawData(float3 vertexA, float3 vertexB, float3 vertexC, bool isPassable)
        {
            vertexA.y += DRAW_HEIGHT_OFFSET;
            vertexB.y += DRAW_HEIGHT_OFFSET;
            vertexC.y += DRAW_HEIGHT_OFFSET;
            Vertices = new Vector3[3] { vertexA, vertexB, vertexC };
            IsPassable = isPassable;
        }

        public TriangleDrawData(TriangleVertices vertices, bool isPassable)
        {
            vertices = vertices.AddHeight(DRAW_HEIGHT_OFFSET);
            Vertices = new Vector3[3] { vertices.PinnaclePos, vertices.LeftBasisPos, vertices.RightBasisPos };
            IsPassable = isPassable;
        }
    }
}
