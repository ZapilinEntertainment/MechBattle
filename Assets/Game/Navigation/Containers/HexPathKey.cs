using System;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle.Navigation
{
    public readonly struct HexPathKey : IEquatable<HexPathKey>
    {
        public readonly HexEdgeKey Start;
        public readonly HexEdgeKey End;

        public HexPathKey(HexEdgeKey start, HexEdgeKey end)
        {
            Start = start;
            End = end;
        }

        public override string ToString() => $"hex path: {Start} -> {End}";
        public static bool operator ==(HexPathKey left, HexPathKey right) => left.Equals(right);
        public static bool operator !=(HexPathKey left, HexPathKey right) => !left.Equals(right);
        public override bool Equals(object obj) => obj is HexPathKey other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(Start, End);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(HexPathKey other) => Start == other.Start && End == other.End;
    }
}
