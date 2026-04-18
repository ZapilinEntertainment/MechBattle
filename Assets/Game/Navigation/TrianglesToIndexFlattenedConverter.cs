using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    // for encoding triangles inside big triangle
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

            _valleyTriangleIndexOffset = CalculateValleyIndexOffset(trianglesPerEdge, _isPeakZone);

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
            //UnityEngine.Debug.Log($"Pinnacles: {pinnaclePos} -> {_startPosPeak} : {_startPosValley}");
        }

        [BurstCompile]
        public static int GetSubdivisionBasisIndex(bool isRight, bool isPeak, int subdivisions)
        {
            var x = (isPeak == isRight) ? 0 : (subdivisions - 1);
            var valleyOffset = CalculateValleyIndexOffset(subdivisions, isPeak);
            return V2ToIndex(new(x, subdivisions - 1)) + (isPeak ? 0 : valleyOffset);
        }

        public NativeArray<byte>.ReadOnly GetRowIndicesTable() => _rowIndicesTable;

        public int TriangularToIndex(IntTriangularPos pos)
        {
            var v2 = TriangularToV2(pos);
            return V2ToIndex(v2, pos.IsPeak);
        }

        public bool TryGetIndex(IntTriangularPos pos, out int index)
        {
            var v2 = TriangularToV2(pos);            
            var isIndexValid = IsV2Valid(v2, pos.IsPeak);
            //UnityEngine.Debug.Log($"{pos} -> {v2} -> {isIndexValid}");
            index = V2ToIndex(v2, pos.IsPeak);
            return isIndexValid;
        }

        public IntTriangularPos IndexToTriangular(int index) 
        { 
            var decodedIndex = DecodeIndex(index);
            var v2 = decodedIndex.xy;
            var isPeak = decodedIndex.z == 1;
            var pos = new int3(-v2.x, v2.y ,v2.x - v2.y) * _signCf;
            //UnityEngine.Debug.Log($"{index} -> {v2} -> {pos} with pinnacle peak at {_startPosPeak}");
            return new(pos + (isPeak ? _startPosPeak : _startPosValley));
        }

        public bool TryGetTriangular(int index, out IntTriangularPos pos)
        {
            var decodedIndex = DecodeIndex(index);
            var v2 = decodedIndex.xy;
            var isPeak = decodedIndex.z == 1;
                        
            if (IsV2Valid(v2.xy, isPeak))
            {
                var posV3 = new int3(-v2.x, v2.y, v2.x - v2.y) * _signCf;
                pos = new(posV3 + (isPeak ? _startPosPeak : _startPosValley));
                return true;
            }
            else
            {
                pos = default;
                return false;
            }
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
            #if UNITY_EDITOR
            if (index < 0 || index >= _rowIndicesTable.Length)
                UnityEngine.Debug.LogError($"invalid index: {index} / valleyOffset: {_valleyTriangleIndexOffset} / total {_trianglesPerEdge * _trianglesPerEdge} / row indices length: {_rowIndicesTable.Length}");   
            #endif
            index = math.clamp(index, 0, _rowIndicesTable.Length-1);
            var y = _rowIndicesTable[index];
            var x = index - y *( y+1) / 2;
            return new(x,y);
        }

        public static NativeArray<byte> FulfilRowIndices(Allocator allocator, int rowsCount)
        {
            var length = rowsCount * (rowsCount + 1) / 2;
            var array = new NativeArray<byte>(length, allocator, NativeArrayOptions.UninitializedMemory);

            array[0] = 0;
            array[1] = 1;
            array[2] = 1;

            var index = 3;
            var i = 2;
            while (i < rowsCount && index < length)
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int V2ToIndex(int2 v2, bool isPeak) => TrianglesToIndexFlattenedConverter.V2ToIndex(v2) + (isPeak ? 0 : _valleyTriangleIndexOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int V2ToIndex(int2 v2) => (v2.y * (v2.y + 1) / 2 + v2.x);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsV2Valid(int2 v2, bool isPeak) =>
            math.all(v2 >= 0) & math.all(v2 < _trianglesPerEdge) & v2.x < v2.y + 1;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int3 DecodeIndex(int index)
        {
            var isPeak = index < _valleyTriangleIndexOffset;
            var correctedIndex = isPeak ? index : (index - _valleyTriangleIndexOffset);            
            var v2 = IndexToV2(correctedIndex);

#if UNITY_EDITOR
            if (correctedIndex >= Length)
                UnityEngine.Debug.LogError($"{index} -> {correctedIndex} -> {v2} of {_trianglesPerEdge} length");
#endif
            return new(v2.x, v2.y, isPeak ? 1 : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CalculateValleyIndexOffset(int trianglesPerEdge, bool isPeakZone)
        {
            var secondaryTypeTrianglesCount = trianglesPerEdge * (trianglesPerEdge - 1) / 2;
            return isPeakZone ? (trianglesPerEdge * trianglesPerEdge - secondaryTypeTrianglesCount) : (secondaryTypeTrianglesCount);
        }
    }
}
