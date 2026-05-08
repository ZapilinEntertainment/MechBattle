using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct IntTriangularPos : IEquatable<IntTriangularPos>
    {
        public const int SERIALIZATION_LENGTH = sizeof(int) * 3;

        public readonly int DownLeft;
        public readonly int Up;
        public readonly int DownRight;

        public int X => DownLeft;
        public int Y => Up;
        public int Z => DownRight;

        public int2 XZ => new(X,Z);
        public int2 XY => new(X,Y);
        public int2 YZ => new(Y,Z);

        public bool IsPeak => (DownLeft + Up + DownRight) % 3 != 1;
        public static IntTriangularPos zero => new(0,0,0);

        public static IntTriangularPos operator + (IntTriangularPos a, int3 delta) =>
            new(a.DownLeft + delta.x, a.Up +delta.y, a.DownRight + delta.z);

        public static IntTriangularPos operator -(IntTriangularPos a, IntTriangularPos delta) =>
           new(a.DownLeft - delta.DownLeft, a.Up - delta.Up, a.DownRight - delta.DownRight);

        public static bool3 operator >(IntTriangularPos a, IntTriangularPos b) => a.ToInt3() > b.ToInt3();
        public static bool3 operator <(IntTriangularPos a, IntTriangularPos b) => a.ToInt3() < b.ToInt3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int3(IntTriangularPos sourceObject) => sourceObject.ToInt3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator IntTriangularPos(int3 v) => new (v.x, v.y, v.z);

        public override string ToString() => $"({DownLeft},{Up},{DownRight}) {(IsPeak ? "peak" : "valley")}";

        public static bool operator ==(IntTriangularPos a, IntTriangularPos b) => a.Equals(b);

        public static bool operator !=(IntTriangularPos a, IntTriangularPos b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (IntTriangularPos)obj;
            return Equals(other);
        }

        public bool Equals(IntTriangularPos other) => DownLeft == other.DownLeft && Up == other.Up && DownRight == other.DownRight;


        public override int GetHashCode() => 
            HashCode.Combine(DownLeft * 11, Up * 17, DownRight * 23);

        // deepseek generated
        // use for vectors only!
        public IntTriangularPos ToStandartizedVector()
        {
            var min = math.min(DownLeft, math.min(Up, DownRight));
            return new IntTriangularPos(DownLeft - min, Up - min, DownRight - min);
        }

        public int3 ToInt3() => new(DownLeft, Up, DownRight);
        public float3 ToFloat3() => new(DownLeft, Up, DownRight);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPointCoordinate() => DownLeft + Up + DownRight == 0;

      

        public IntTriangularPos(int downLeft, int up, int downRight)
        {
            DownLeft = downLeft;
            DownRight = downRight;
            Up = up;
        }

        public IntTriangularPos(int3 pos) : this(pos.x, pos.y, pos.z) { }
    }
}
