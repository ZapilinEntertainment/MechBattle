using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle.Navigation
{
    public struct SquaredHexTrianglesList<T> where T : unmanaged
    {
        public int Length => _data.Length;
        public TrianglesToIndexSquaredConverter CoordsConverter { get; private set; }  

        private NativeArray<T> _data;
        

        public SquaredHexTrianglesList(NativeArray<T> data, TrianglesToIndexSquaredConverter converter)
        {
            _data = data;
            CoordsConverter = converter;
        }

        public T this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        /*  row1 row2 
         *    AV Av      AVA
         *   aV AV       VAV   --> X axis here
         *  example of hex with radius of 1. Small letters represent non-hex triangles
         *  
         *  A(0,2) V(0,3)     A(1,2) v(1,3)
         *  a(0,1) V(0,2)     A(1,0) V(1,1)
         */

        public bool TryGet(IntTriangularPos pos, out T result)
        {
            if (!TryGetIndex(pos, out var index))
            {
                result = default;
                return false;
            }

            result = _data[index];
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetIndex(IntTriangularPos pos, out int index) => CoordsConverter.TryConvertToIndex(pos, out index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValidOrDefault(IntTriangularPos pos) => CoordsConverter.TryConvertToIndex(pos, out var index) ? _data[index] : default;

        public void Set(IntTriangularPos pos, T value) 
        {
            var index = CoordsConverter.TriangularToIndex(pos);
            if (IsIndexValid(index)) 
                _data[index] = value;
        }        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public bool IsIndexValid(int index) => (uint)index < (uint)Length;
    }
}
