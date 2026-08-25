using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct MechPartitionComponent : IComponent 
    {
        public readonly Entity MechEntity;
        public readonly MechPartitionKey Key;
    
        public MechPartitionComponent(Entity entity, MechPartitionKey key)
        {
            MechEntity = entity;
            Key = key;
        }
    }
}