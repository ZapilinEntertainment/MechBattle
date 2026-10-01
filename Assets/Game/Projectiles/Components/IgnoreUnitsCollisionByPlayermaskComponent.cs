using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct IgnoreUnitsCollisionByPlayermaskComponent : IComponent 
    {
        public readonly PlayerRelationsMask PlayersMask;

        public IgnoreUnitsCollisionByPlayermaskComponent(PlayerRelationsMask mask) => PlayersMask = mask;
    
    }
}