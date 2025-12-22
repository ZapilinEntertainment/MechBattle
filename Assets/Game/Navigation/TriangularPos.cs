using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct IntTriangularPos
    {
        public readonly int DownLeft;
        public readonly int Up;
        public readonly int DownRight;
        public readonly bool IsPeak;
        private const float SQRT_THREE_D3 = 0.5773502f; // sqrt(3) / 3f

        public static IntTriangularPos operator + (IntTriangularPos a, int3 delta) =>
            new(a.DownLeft + delta.x, a.Up +delta.y, a.DownRight + delta.z);

        public static implicit operator int3(IntTriangularPos sourceObject) => sourceObject.ToInt3();

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
            return DownLeft == other.DownLeft && Up == other.Up && DownRight == other.DownRight;
        }

       
        public override int GetHashCode() => 
            HashCode.Combine(DownLeft * 11, Up * 17, DownRight * 23);

        // deepseek generated
        public IntTriangularPos ToStandartized()
        {
            var min = math.min(DownLeft, math.min(Up, DownRight));
            return new IntTriangularPos(DownLeft - min, Up - min, DownRight - min);
        }

        public int3 ToInt3() => new(DownLeft, Up, DownRight);
        public float3 ToFloat3() => new(DownLeft, Up, DownRight);

        public IntTriangularPos(int downLeft, int up, int downRight)
        {
            DownLeft = downLeft;
            DownRight = downRight;
            Up = up;
            IsPeak = (DownLeft + Up + DownRight) % 3 != 1;
        }

        public IntTriangularPos(int3 pos) : this(pos.x, pos.y, pos.z) { }
    }
}
