using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct NextStepPositionCalculationRequest : IComponent 
    {
        public readonly Entity ChassisEntity;
        public readonly Entity BackLeg;

        public NextStepPositionCalculationRequest(Entity chassisEntity, Entity backLeg)
        {
            ChassisEntity = chassisEntity;
            BackLeg = backLeg;
        }
    }
}