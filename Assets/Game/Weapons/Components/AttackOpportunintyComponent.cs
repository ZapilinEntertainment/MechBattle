using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]

    // describes how close entity to switch into attack state
    // depends on distance to recommended distance ratio
    // is gun loaded, is fireline clear
    public struct AttackOpportunintyComponent : IComponent 
    {
        public float Value;    
    }
}