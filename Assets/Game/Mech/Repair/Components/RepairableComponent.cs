using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct RepairableComponent : IComponent 
    {
        public readonly Entity RepairProducingEntity;
        public readonly RepairPriority Priority;

        public RepairableComponent(Entity repairTeamEntity, RepairPriority priority)
        {
            RepairProducingEntity = repairTeamEntity;
            Priority = priority;
        }
    
    }
}