using System;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using System.Runtime.CompilerServices;
using TriInspector;

namespace ZE.MechBattle.Navigation
{
    [Serializable]
    public struct HexPathNodeKey : IEquatable<HexPathNodeKey>
    {
        [ShowInInspector] private string SerializedOutput => ToString();

        public int2 HexCoord => _value.xy;
        public HexEdge Edge => (HexEdge)_value.z;
        public int EdgeIndex => _value.z;
        
        public float2 EdgeCenterHexCoord => _value.xy + Edge.ToEdgePosOffsetVector();

        private readonly int3 _value;

        public HexPathNodeKey(int hexCoordX, int hexCoordY, HexEdge edge)
        {
            _value = new int3(hexCoordX, hexCoordY, (int)edge);
        }

        public HexPathNodeKey(int2 hexPos, HexEdge edge)
        {
            _value = new int3(hexPos, (int)edge);
        }

        public HexPathNodeKey(int2 hexPos, int edge)
        {
            _value = new int3(hexPos, edge);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(HexPathNodeKey other) => math.all(_value == other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => (int)math.hash(_value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int2 ToNextHexCoord() => _value.xy + Edge.ToHexOffsetVector();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HexPathNodeKey ToOpposite() => new(ToNextHexCoord(), Edge.ToOpposite());


        public static bool operator ==(HexPathNodeKey left, HexPathNodeKey right) => left.Equals(right);
        public static bool operator !=(HexPathNodeKey left, HexPathNodeKey right) => !left.Equals(right);
        public override bool Equals(object obj) => obj is HexPathNodeKey other && Equals(other);
        public override string ToString() => $"{HexCoord}:{Edge}";
    }
}
