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
        public readonly IntTriangularPos BottomLeftPeakTrianglePos;
        public readonly IntTriangularPos BottomLeftValleyTrianglePos;

        public TrianglesToIndexConverter(IntTriangularPos hexCenterInTriangular, int trianglesPerEdge)
        {
            ArrayWidth = trianglesPerEdge * 2; // count only A triangles
            ArrayHeight = trianglesPerEdge * 4; // count both A and V triangles

            BottomLeftPeakTrianglePos = hexCenterInTriangular + new int3(2 * trianglesPerEdge - 1, - trianglesPerEdge,  -trianglesPerEdge);
            BottomLeftValleyTrianglePos = TriangularMath.GetPeakNeighbour(BottomLeftPeakTrianglePos, PeakNeighbour.EdgeUpRight);
        }

        public int TriangularToIndex(IntTriangularPos pos)
        {
            var delta = pos.IsPeak ? (pos - BottomLeftPeakTrianglePos) : (pos - BottomLeftValleyTrianglePos);
            var y = delta.Y * 2 + (pos.IsPeak ? 0 : 1);
            var x = delta.Z;

            //Debug.Log($"{pos} -> ({x},{y})");
            return x * ArrayHeight + y;
        }

        public IntTriangularPos IndexToTriangular(int index)
        {
            var pos2d = new int2(index / ArrayHeight, index % ArrayHeight);
            var delta = new int3(-pos2d.y / 2, pos2d.y / 2, 0) + new int3(-pos2d.x, 0, pos2d.x);
            var startPos = pos2d.y % 2 == 0 ? BottomLeftPeakTrianglePos : BottomLeftValleyTrianglePos;

            //Debug.Log($"> {index} : {pos2d} : {startPos} + {delta}");
            return new (delta + startPos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int2 TriangularToVector2D(IntTriangularPos pos) => math.select(
                new int2(pos.Z, pos.Y * 2),
                new int2(pos.Z, pos.Y * 2 + 1),
                pos.IsPeak);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIndexValid(int index) => (uint)index < (uint)(ArrayWidth * ArrayHeight);
    }
}
