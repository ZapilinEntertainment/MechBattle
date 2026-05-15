using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct float6 : IEquatable<float6>
    {
        private fixed float _values[6];

        public float6(float f1, float f2, float f3, float f4, float f5, float f6)
        {
            _values[0] = f1;
            _values[1] = f2;
            _values[2] = f3;
            _values[3] = f4;
            _values[4] = f5;
            _values[5] = f6;
        }

        public float6(float x)
        {
            _values[0] = x;
            _values[1] = x;
            _values[2] = x;
            _values[3] = x;
            _values[4] = x;
            _values[5] = x;
        }

        public readonly bool Equals(float6 other)
        {
            return _values[0] == other[0]
                && _values[1] == other[1]
                && _values[2] == other[2]
                && _values[3] == other[3]
                && _values[4] == other[4]
                && _values[5] == other[5];
        }

        public override readonly bool Equals(object obj) => obj is float6 other && Equals(other);

        public override readonly int GetHashCode()
        {
            var hash = new HashCode();
            for (int i = 0; i < 6; i++)
            {
                hash.Add(_values[i]); 
            }
            return hash.ToHashCode();
        }

        public static bool operator ==(float6 left, float6 right) => left.Equals(right);
        public static bool operator !=(float6 left, float6 right) => !left.Equals(right);

        public override readonly string ToString() => $"float6({_values[0]}, {_values[1]}, {_values[2]}, {_values[3]}, {_values[4]}, {_values[5]})";

        public float this[int index]
        {
            get
            {
                if ((uint)index >= 6) throw new System.ArgumentOutOfRangeException();
                return _values[index];
            }
            set
            {
                if ((uint)index >= 6) throw new System.ArgumentOutOfRangeException();
                _values[index] = value;
            }
        }

        public float this[HexEdge edge]
        {
            get => _values[(int)edge];
            set => _values[(int)edge]= value;
        }

    }

}
