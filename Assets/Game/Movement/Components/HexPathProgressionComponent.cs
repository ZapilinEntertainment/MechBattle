using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct HexPathProgressionComponent : IComponent 
    {
        public readonly int StepsCount;
        public int StepIndex;

        public HexPathProgressionComponent(int stepsCount)
        {
            StepsCount = stepsCount;
            StepIndex = 0;
        }
    }
}