using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public readonly struct TriangleDrawData
    {
        public readonly TriangleVertices Vertices;
        public readonly bool IsPassable;
        private const float DRAW_HEIGHT_OFFSET = 0.01f;

        public TriangleDrawData(in TriangleVertices vertices, bool isPassable)
        {
            Vertices = vertices;
            Vertices.AddHeight(DRAW_HEIGHT_OFFSET);

            IsPassable = isPassable;
        }
    }
}
