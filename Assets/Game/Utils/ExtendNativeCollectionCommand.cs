using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.Utils
{
    public static class ExtendNativeCollectionCommand
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeParallelHashMap<TKey, TValue> Execute<TKey, TValue>(NativeParallelHashMap<TKey,TValue> map, int newCapacity, Allocator allocator, bool clear = true)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            var rebuild = false;

            if (map.IsCreated)
            {
                if (map.Capacity < newCapacity)
                {
                    map.Dispose();
                    rebuild = true;
                }
                else
                {
                    if (clear)
                        map.Clear();
                }
            }
            else
            {
                rebuild = true;
            }

            if (rebuild)
                return new NativeParallelHashMap<TKey, TValue>(math.ceilpow2(newCapacity), allocator);
            else
                return map;
        }

        // copy
        public static NativeHashMap<TKey, TValue> Execute<TKey, TValue>(NativeHashMap<TKey, TValue> map, int newCapacity, Allocator allocator, bool clear = true)
           where TKey : unmanaged, IEquatable<TKey>
           where TValue : unmanaged
        {
            var rebuild = false;

            if (map.IsCreated)
            {
                if (map.Capacity < newCapacity)
                {
                    map.Dispose();
                    rebuild = true;
                }
                else
                {
                    if (clear)
                        map.Clear();
                }
            }
            else
            {
                rebuild = true;
            }

            if (rebuild)
                return new NativeHashMap<TKey, TValue>(math.ceilpow2(newCapacity), allocator);
            else
                return map;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<TValue> Execute<TValue>(NativeArray<TValue> array, int newCapacity, Allocator allocator)
            where TValue : unmanaged
        {
            var rebuild = false;

            if (array.IsCreated)
            {
                if (array.Length < newCapacity)
                {
                    array.Dispose();
                    rebuild = true;
                }
            }
            else
            {
                rebuild = true;
            }

            if (rebuild)
                return new NativeArray<TValue>(math.ceilpow2(newCapacity), allocator);
            else
                return array;
        }
    }
}
