using System;
using System.Buffers;

namespace ZE.MechBattle.Navigation
{
    public struct DisposableArray<T> : IDisposable
    {
        public readonly T[] Values;
        public readonly int Length;

        public DisposableArray(int length)
        {
            Length = length;
            Values = ArrayPool<T>.Shared.Rent(Length);
        }

        public DisposableArray(T[] array, int length)
        {
            Values = array; 
            Length = length;
        }

        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(Values);
        }

        public T this[int index]
        {
            get => Values[index];
            set => Values[index] = value;
        }
    }
}
