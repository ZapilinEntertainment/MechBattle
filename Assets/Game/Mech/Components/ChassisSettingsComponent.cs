using Scellecs.Morpeh;
using Unity.Burst;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [BurstCompile]
    public readonly struct ChassisSettingsComponent : IComponent 
    {
        public readonly StepSettings StepSettings;
        public readonly ChassisSettings ChassisSettings;
        public readonly float2 FootSize;
    
        public ChassisSettingsComponent(ChassisSettings chassis, StepSettings step, float2 footSize)
        {
            StepSettings = step;
            ChassisSettings = chassis;
            FootSize = footSize;
        }
    }
}