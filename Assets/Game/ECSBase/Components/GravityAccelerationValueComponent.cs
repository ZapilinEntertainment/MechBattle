using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct GravityAccelerationValueComponent : IComponent 
    {
        public readonly float AffectionCf;
        public float CurrentValue;    

        public GravityAccelerationValueComponent(float affectionCf)
        {
            AffectionCf = affectionCf;
            CurrentValue = 0f;
        }
    }
}