using Scellecs.Morpeh;
using UnityEngine;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs 
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct HexPathSelectRequestComponent : IComponent 
    {
        public readonly int2 StartHex;
        public readonly int2 EndHex;
        public readonly HexEdgesMask StartEdgesMask;
        public readonly HexEdgesMask EndEdgesMask;

        public HexPathSelectRequestComponent(int2 startHex, HexEdgesMask startMask, int2 endHex, HexEdgesMask endMask)
        {
            StartHex = startHex;
            EndHex = endHex;
            StartEdgesMask = startMask;
            EndEdgesMask = endMask;
        }
    }
}