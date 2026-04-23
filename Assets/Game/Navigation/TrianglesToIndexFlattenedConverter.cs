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
        private readonly int _trianglesPerEdge;
        private readonly NativeArray<byte>.ReadOnly _rowIndicesTable;
        private readonly TriangleEnumerationSettings _settings;

        public TrianglesToIndexFlattenedConverter(IntTriangularPos pinnaclePos, int trianglesPerEdge, NativeArray<byte>.ReadOnly rowIndicesTable)
        {
            _rowIndicesTable = rowIndicesTable;
            _trianglesPerEdge = trianglesPerEdge;
            _settings = new(pinnaclePos, trianglesPerEdge);
        }

        [BurstCompile]
        public static int GetSubdivisionBasisIndex(bool isRight, bool isPeak, int subdivisions)
        {
            var x = (isPeak == isRight) ? 0 : (subdivisions - 1);
            var valleyOffset = TrianglesEnumerationLogic.CalculateValleyIndexOffset(subdivisions, isPeak);
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
            return _settings.V2ToTriangular(decodedIndex.xy, decodedIndex.z == 1);
        }

        public bool TryGetTriangular(int index, out IntTriangularPos pos)
        {
            var decodedIndex = DecodeIndex(index);
            var v2 = decodedIndex.xy;
            var isPeak = decodedIndex.z == 1;
                        
            if (IsV2Valid(v2.xy, isPeak))
            {
                var posV3 = new int3(-v2.x, v2.y, v2.x - v2.y) * _settings.SignCf;
                pos = new(posV3 + _settings.GetStart(isPeak));
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

            var startPos = _settings.GetStart(pos.IsPeak);
            var xdelta = startPos.x - pos.X; // horizontal shift: (-1,0,1)  for valley / (1,0,-1) for peak
            var ydelta = pos.Y - startPos.y; // vertical shift: (0,-1,1) for peak / (0,1,-1) for valley
            
            var v2 = new int2(xdelta, ydelta) * _settings.SignCf;
            return v2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int2 IndexToV2(int index)
        {
            #if UNITY_EDITOR
            if (index < 0 || index >= _rowIndicesTable.Length)
                UnityEngine.Debug.LogError($"invalid index: {index} / valleyOffset: {_settings.ValleysIndexOffset} / total {_trianglesPerEdge * _trianglesPerEdge} / row indices length: {_rowIndicesTable.Length}");   
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
            if (rowsCount == 1)
                return array;

            array[1] = 1;
            array[2] = 1;

            if (rowsCount == 2)
                return array;

            var index = 3;
            var rowValue = 2;
            while (rowValue < rowsCount && index < length)
            {
                var byteIndex = (byte)rowValue;
                var j = -1;
                while (j < rowValue && index < length)
                {
                    array[index++] = byteIndex;
                    j++;
                }

                rowValue++;
            }

            return array;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int V2ToIndex(int2 v2, bool isPeak) => TrianglesToIndexFlattenedConverter.V2ToIndex(v2) + (isPeak ? 0 : _settings.ValleysIndexOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int V2ToIndex(int2 v2) => (v2.y * (v2.y + 1) / 2 + v2.x);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsV2Valid(int2 v2, bool isPeak) =>
            math.all(v2 >= 0) & math.all(v2 < _trianglesPerEdge) & v2.x < v2.y + 1;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int3 DecodeIndex(int index)
        {
            var correctedIndex = _settings.GetTypedIndex(index ,out var isPeak);         
            var v2 = IndexToV2(correctedIndex);

#if UNITY_EDITOR
            if (correctedIndex >= Length)
                UnityEngine.Debug.LogError($"{index} -> {correctedIndex} -> {v2} of {_trianglesPerEdge} length");
#endif
            return new(v2.x, v2.y, isPeak ? 1 : 0);
        }

        
    }
}
