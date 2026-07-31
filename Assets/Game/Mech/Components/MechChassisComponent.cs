using Scellecs.Morpeh;
using Unity.Burst;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [BurstCompile]
    public struct MechChassisComponent : IComponent 
    {
        public LegDataContainer<Entity> LeftLeg;
        public LegDataContainer<Entity> RightLeg;    
    }
}