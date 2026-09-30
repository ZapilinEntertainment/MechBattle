using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct SquadMemberComponent : IComponent 
    {
        public readonly Entity SquadEntity;
        public int Index;

        public SquadMemberComponent(Entity squadEntity, int index)
        {
            SquadEntity = squadEntity;
            Index = index;
        }
    
    }
}