using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle.Navigation
{
    public struct SquaredHexTrianglesList<T> : IDisposable where T : unmanaged
    {
        public readonly int Length;
        public readonly TrianglesToIndexConverter CoordsConverter;

        private NativeArray<T> _data;
        

        public SquaredHexTrianglesList(IntTriangularPos hexCenterInTriangular, int trianglesPerEdge, Allocator allocator)
        {
            CoordsConverter = new(hexCenterInTriangular, trianglesPerEdge);

            Length = CoordsConverter.ArrayHeight * CoordsConverter.ArrayWidth;
            _data = new NativeArray<T>(Length, allocator);            
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
            var index = CoordsConverter.TriangularToIndex(pos);
            if (IsIndexValid(index))
            {
                result = _data[index];
                return true;
            }

            result = default;
            return false;
        }

        public void Set(IntTriangularPos pos, T value) 
        {
            var index = CoordsConverter.TriangularToIndex(pos.ToStandartized());
            if (IsIndexValid(index)) 
                _data[index] = value;
        }        

        public void Dispose() 
        {
            if (_data.IsCreated)
                _data.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIndexValid(int index) => (uint)index < (uint)Length;
    }
}
