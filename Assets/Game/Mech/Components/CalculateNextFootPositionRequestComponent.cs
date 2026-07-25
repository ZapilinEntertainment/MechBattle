using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct CalculateNextFootPositionRequestComponent : IComponent 
    {
        public readonly Entity MovingLeg;
        public readonly Entity BackLeg;    

        public CalculateNextFootPositionRequestComponent(Entity movingLeg, Entity backLeg)
        {
            MovingLeg = movingLeg;
            BackLeg = backLeg;
        }
    }
}