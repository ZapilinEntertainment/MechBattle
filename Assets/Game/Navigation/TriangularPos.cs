using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public struct TriangularPos
    {
        public float DownLeft;
        public float Up;
        public float DownRight;       
    }

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
            // why equalize Peak - we can invalidate struct simple by setting peak to wrong
            return DownRight == other.DownRight && Up == other.Up && DownLeft == other.DownLeft && IsPeak == other.IsPeak;
        }
        public override int GetHashCode() => HashCode.Combine(DownRight, Up, DownLeft);

        public int3 ToInt3() => new(DownLeft, Up, DownRight);

        public IntTriangularPos(int downLeft, int up, int downRight)
        {
            DownLeft = downLeft;
            DownRight = downRight;
            Up = up;
            IsPeak = (DownLeft + Up + DownRight) % 3 != 1;
        }

        public IntTriangularPos(int3 pos) : this(pos.x, pos.y, pos.z) { }

        public IntTriangularPos (float cartesianX, float cartesianZ) : this(
            (int)math.ceil((-1 * cartesianX - SQRT_THREE_D3 * cartesianZ)),
            (int)math.floor(SQRT_THREE_D3 * 2 * cartesianZ) + 1,
            (int)math.ceil((1 * cartesianX - SQRT_THREE_D3 * cartesianZ)))
        { }

        public IntTriangularPos Standartize()
        {
            var pos = ToInt3();
            var neg = math.min(pos, 0);

            var absNeg = math.abs(neg);
            var sum = absNeg.x + absNeg.y + absNeg.z;

            pos += sum - absNeg;
            return new(math.max(pos, 0));
        }
    }
}
