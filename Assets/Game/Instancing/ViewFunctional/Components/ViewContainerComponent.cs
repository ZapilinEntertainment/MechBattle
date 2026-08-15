using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using TriInspector;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct ViewContainerComponent : IComponent 
    {
        public readonly int Id;
#if UNITY_EDITOR
        [ShowInInspector] private int _id => Id;
#endif

        public ViewContainerComponent(int id) => Id = id;
    
    }
}