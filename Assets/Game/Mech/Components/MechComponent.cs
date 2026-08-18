using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct MechComponent : IComponent 
    {
        public readonly Entity ChassisEntity;
        public readonly Entity UpperPartEntity;
        public readonly Entity HeadEntity;

        public MechComponent(Entity chassisEntity, Entity upperPartEntity, Entity headEntity)
        {
            ChassisEntity = chassisEntity;
            UpperPartEntity = upperPartEntity;
            HeadEntity = headEntity;
        }
    
    }
}