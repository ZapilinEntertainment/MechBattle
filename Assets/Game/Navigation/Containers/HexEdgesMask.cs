using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public readonly struct HexEdgesMask
    {
        private readonly BitField32 _value;

        public HexEdgesMask(int value) => _value = new((uint)value);
        public HexEdgesMask(uint value) => _value = new(value);
        public HexEdgesMask(BitField32 value) => _value = value;
        public HexEdgesMask(bool value) : this(value ? uint.MaxValue : uint.MinValue) { }

        public HexEdgesMask SetEdgeStatus(HexEdge edge, bool isPresented)
        {
            var next = _value;
            next.SetBits((int)edge, isPresented);
            return new HexEdgesMask(next);
        }

        public bool IsEdgePresented(HexEdge edge) => _value.IsSet((int)edge);
        public bool IsEdgePresented(int edgeIndex) => _value.IsSet(edgeIndex);

        public static HexEdgesMask operator &(HexEdgesMask a, HexEdgesMask b) => new (a._value.Value & b._value.Value);
        public static HexEdgesMask operator |(HexEdgesMask a, HexEdgesMask b) => new (a._value.Value | b._value.Value);
        public static uint operator |(uint a, HexEdgesMask b) => a | b._value.Value;

        public bool HasOverlapsWith(HexEdgesMask mask) => (_value.Value & mask._value.Value) != 0;

        public override string ToString()
        {
            const int ALL_EDGES_MASK = 0b_111111;
            if ((_value.Value & ALL_EDGES_MASK) == ALL_EDGES_MASK)
                return "All";

            var stringBuilder = new System.Text.StringBuilder();
            var stringsCount = 0;
            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                if (IsEdgePresented(edge))
                {
                    stringBuilder.Append(edge.ToString());
                    stringBuilder.Append(' ');
                    stringsCount++;
                }                    
            }
            return stringsCount == 0 ? "no edges" : stringBuilder.ToString();
        }

    }
}
