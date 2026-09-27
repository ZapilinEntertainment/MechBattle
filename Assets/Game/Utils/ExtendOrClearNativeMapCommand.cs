using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.Utils
{
    public static class ExtendOrClearNativeMapCommand
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeParallelHashMap<TKey, TValue> Execute<TKey, TValue>(NativeParallelHashMap<TKey,TValue> map, int newCapacity, Allocator allocator)
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
    
    }
}
