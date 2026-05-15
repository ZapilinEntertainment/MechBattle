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
        public readonly HexPathSearchRequest Value;

        public HexPathSelectRequestComponent(in HexPathSearchRequest request)
        {
            Value = request;
        }
    }
}