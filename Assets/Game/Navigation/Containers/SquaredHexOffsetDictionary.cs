using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct SquaredHexOffsetDictionary : IDisposable
    {
        public static readonly IntTriangularPos[] PeakNeighbours = new IntTriangularPos[12]
      {
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexUp),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexUpRight),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.EdgeUpRight),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexRight),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexDownRightValley),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexDownRightPeak),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.EdgeDown),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexDownLeftPeak),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexDownLeftValley),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexLeft),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.EdgeUpLeft),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexUpLeft),
      };

        public static readonly IntTriangularPos[] ValleyNeighbours = new IntTriangularPos[12]
        {
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.EdgeUp),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexUpRightValley),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexUpRightPeak),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexRight),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.EdgeDownRight),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexDownRight),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexDown),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexDownLeft),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.EdgeDownLeft),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexLeft),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexUpLeftPeak),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexUpLeftValley),
        };
        private readonly NativeArray<int2> _offsets;

        public SquaredHexOffsetDictionary(in TrianglesToIndexConverter converter, Allocator allocator)
        {
            _offsets = new NativeArray<int2>(24, allocator, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < 12; i++)
            {
                _offsets[i] = converter.TriangularToVector2D(PeakNeighbours[i]);
                _offsets[i + 12] = converter.TriangularToVector2D(ValleyNeighbours[i]);
            }
        }

        public void Dispose()
        {
            if (_offsets.IsCreated)
                _offsets.Dispose();
        }
    }
}
