using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public readonly struct HexEdgesMask
    {
        private readonly BitField32 _bitfield;

        public HexEdgesMask(int value) => _bitfield = new((uint)value);
        public HexEdgesMask(uint value) => _bitfield = new(value);
        public HexEdgesMask(BitField32 value) => _bitfield = value;

        public HexEdgesMask SetEdgeStatus(HexEdge edge, bool isPresented)
        {
            var next = _bitfield;
            next.SetBits((int)edge, isPresented);
            return new HexEdgesMask(next);
        }

        public bool IsEdgePresented(HexEdge edge) => _bitfield.IsSet((int)edge);
        public bool IsEdgePresented(int edgeIndex) => _bitfield.IsSet(edgeIndex);

        public static HexEdgesMask operator &(HexEdgesMask a, HexEdgesMask b) => new (a._bitfield.Value & b._bitfield.Value);
        public static HexEdgesMask operator |(HexEdgesMask a, HexEdgesMask b) => new (a._bitfield.Value | b._bitfield.Value);
        public static uint operator |(uint a, HexEdgesMask b) => a | b._bitfield.Value;

        public bool HasOverlapsWith(HexEdgesMask b) => (_bitfield.Value & b._bitfield.Value) != 0;

    }
}
