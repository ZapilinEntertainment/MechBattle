using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct RegularTrianglePathProgressionComponent : IComponent 
    {
        public readonly int TotalStepsCount;
        public int StepIndex;

        public RegularTrianglePathProgressionComponent(int stepsCount)
        {
            TotalStepsCount = stepsCount;
            StepIndex = 0;
        }

    }
}