using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    [BurstCompile]
    public readonly struct TrianglesToIndexFlattenedConverter
    {
        public int Length => _trianglesPerEdge * _trianglesPerEdge;

        /*
         Peaks and valleys have parralel v2 coords and indices
         Resulting valley indices = index + _valleyTriangleIndexOffset
         Peak:
                            [0]=(0,0)A
                   [3]=(1,1)A [2]=(0,0)V [1]=(0,1)A
        [8]=(2,2)A [7]=(1,2)V [6]=(1,2)A [5]=(0,2)V [4]=(0,2)A

        Valley:

        [4]=(0,2)V [5]=(0,2)A [6]=(1,2)V [7]=(1,2)A [8]=(2,2)V
                   [1]=(0,1)V [2]=(0,0)A [3]=(1,1)V
                              [0]=(0,0)V
        */
        private readonly int3 _startPosPeak;
        private readonly int3 _startPosValley;
        private readonly int _trianglesPerEdge;
        private readonly int _signCf;
        private readonly int _valleyTriangleIndexOffset;
        private readonly bool _isPeakZone;
        private readonly NativeArray<byte>.ReadOnly _rowIndicesTable;

        public TrianglesToIndexFlattenedConverter(IntTriangularPos pinnaclePos, int trianglesPerEdge, NativeArray<byte>.ReadOnly rowIndicesTable)
        {
            _rowIndicesTable = rowIndicesTable;
            _trianglesPerEdge = trianglesPerEdge;

            _isPeakZone = pinnaclePos.IsPeak;            
            _signCf = _isPeakZone ? -1 : 1;

            var secondaryTypeTrianglesCount = _trianglesPerEdge * (_trianglesPerEdge - 1) / 2;
            _valleyTriangleIndexOffset = _isPeakZone ? (_trianglesPerEdge * _trianglesPerEdge - secondaryTypeTrianglesCount) : (secondaryTypeTrianglesCount);

            if (_isPeakZone)
            {
                _startPosPeak = pinnaclePos;
                _startPosValley = TriangularMath.GetPeakNeighbour(pinnaclePos, PeakNeighbour.EdgeDown);
            }
            else
            {
                _startPosValley = pinnaclePos;
                _startPosPeak = TriangularMath.GetValleyNeighbour(pinnaclePos, ValleyNeighbour.EdgeUp);
            }
            
        }

        public int TriangularToIndex(IntTriangularPos pos)
        {
            var v2 = TriangularToV2(pos);
            return (v2.y * (v2.y +1) / 2 + v2.x) + (pos.IsPeak ? 0 : _valleyTriangleIndexOffset);
        }

        public IntTriangularPos IndexToTriangular(int index) 
        { 
            var isPeak = index < _valleyTriangleIndexOffset;
            var correctedIndex = isPeak ? index : (index - _valleyTriangleIndexOffset);
            var v2 = IndexToV2(correctedIndex);
            var pos = new int3(-v2.x, v2.y ,v2.x - v2.y) * _signCf;
            return new(pos + (isPeak ? _startPosPeak : _startPosValley));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int2 TriangularToV2(IntTriangularPos pos)
        {
            // note: peaks and valleys have own v2 coordinates
            // there can be (0,0) peak and (0,0) valley simultaneously

            var startPos = pos.IsPeak ? _startPosPeak : _startPosValley;
            var xdelta = startPos.x - pos.X; // horizontal shift: (-1,0,1)  for valley / (1,0,-1) for peak
            var ydelta = pos.Y - startPos.y; // vertical shift: (0,-1,1) for peak / (0,1,-1) for valley
            
            var v2 = new int2(xdelta, ydelta) * _signCf;
            return v2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int2 IndexToV2(int index)
        {
            var y = _rowIndicesTable[index];
            var x = index - y *( y+1) / 2;
            return new(x,y);
        }

        public static NativeArray<byte> FulfilRowIndices(Allocator allocator, int maxRowIndex)
        {
            var length = maxRowIndex * (maxRowIndex + 1) / 2;
            var array = new NativeArray<byte>(length, allocator, NativeArrayOptions.UninitializedMemory);

            array[0] = 0;
            array[1] = 1;
            array[2] = 1;

            var index = 3;
            var i = 2;
            while (i < maxRowIndex && index < length)
            {
                var byteIndex = (byte)i;
                var j = -1;
                while (j < i && index < length)
                {
                    array[index++] = byteIndex;
                    j++;
                }

                i++;
            }

            return array;
        }
    }
}
