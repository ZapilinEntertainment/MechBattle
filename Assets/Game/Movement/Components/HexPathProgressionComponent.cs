using Unity.IL2CPP.CompilerServices;
using TriInspector;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct HexPathProgressionComponent : IHexPathComponent
    {
        [ShowInInspector, ReadOnly] public readonly int StepsCount;
        public int StepIndex;
        public int2 TargetHexCoord;

        public HexPathProgressionComponent(int stepsCount, int2 targetHexCoord)
        {
            StepsCount = stepsCount;
            StepIndex = 0;
            TargetHexCoord = targetHexCoord;
        }
    }
}