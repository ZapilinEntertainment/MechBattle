using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle.Navigation
{
    public readonly struct TrianglesToIndexConverter
    {
        public readonly int ArrayWidth;
        public readonly int ArrayHeight;
        private readonly IntTriangularPos BottomLeftCornerPosStandartized;

        public TrianglesToIndexConverter(IntTriangularPos hexCenterInTriangular, int trianglesPerEdge)
        {
            ArrayWidth = trianglesPerEdge * 2; // count only A triangles
            ArrayHeight = trianglesPerEdge * 4; // count both A and V triangles

            BottomLeftCornerPosStandartized = (hexCenterInTriangular + trianglesPerEdge * new int3(2, -1, -1)).ToStandartized();
        }

        public int TriangularToIndex(IntTriangularPos pos)
        {
            var delta = pos.ToStandartized() - BottomLeftCornerPosStandartized;
            var pos2d = TriangularToVector2D(delta);

            return pos2d.x * ArrayHeight + pos2d.y;
        }

        public IntTriangularPos IndexToTriangular(int index)
        {
            var pos2d = new int2(index / ArrayHeight, index % ArrayHeight);
            return new(BottomLeftCornerPosStandartized.X, BottomLeftCornerPosStandartized.Y + pos2d.y, BottomLeftCornerPosStandartized.Z + pos2d.x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int2 TriangularToVector2D(IntTriangularPos pos) => math.select(
                new int2(pos.Z, pos.Y * 2),
                new int2(pos.Z, pos.Y * 2 + 1),
                pos.IsPeak);
    }
}
