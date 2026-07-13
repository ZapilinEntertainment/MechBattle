using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using TriInspector;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct ParentEntityComponent : IComponent 
    {
        public readonly Entity Value;
        public ParentEntityComponent(Entity parent) => Value = parent;

#if UNITY_EDITOR
        [ShowInInspector] private int parentId => Value.Id;
#endif

    }
}