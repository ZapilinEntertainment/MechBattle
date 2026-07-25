using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Burst;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [BurstCompile]
    public struct MechInputComponent : IComponent 
    {
        public float SpeedValue;
        public float SteerValue;    
    }
}