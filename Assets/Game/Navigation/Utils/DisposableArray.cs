using System;
using System.Buffers;

namespace ZE.MechBattle.Navigation
{
    public class DisposableArray : IDisposable
    {
        public readonly int[] Values;

        public DisposableArray(int length)
        {
            Values = ArrayPool<int>.Shared.Rent(length);
        }

        public void Dispose()
        {
            ArrayPool<int>.Shared.Return(Values);
        }

        public int this[int index]
        {
            get => Values[index];
            set => Values[index] = value;
        }
    }
}
