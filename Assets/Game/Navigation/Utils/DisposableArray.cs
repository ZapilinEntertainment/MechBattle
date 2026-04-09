using System;
using System.Buffers;

namespace ZE.MechBattle.Navigation
{
    public class DisposableArray<T> : IDisposable
    {
        public readonly T[] Values;

        public DisposableArray(int length)
        {
            Values = ArrayPool<T>.Shared.Rent(length);
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
