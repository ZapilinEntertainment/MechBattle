using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public readonly struct HexEdgesMask
    {
        private readonly BitField32 _value;

        public HexEdgesMask(int value) => _value = new((uint)value);

        public void SetEdgeStatus(HexEdge edge, bool isPresented) => _value.SetBits((int)edge, isPresented);
        public void IsEdgePresented(HexEdge edge) => _value.IsSet((int)edge);
    
    }
}
