using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs 
{

    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct RegularHexPathComponent : IComponent 
    {
        public readonly int PathId;
        public readonly int StepsCount;
        public int StepIndex;   

        public RegularHexPathComponent(int pathId,  int stepsCount)
        {
            PathId = pathId;
            StepIndex = 0;
            StepsCount = stepsCount;
        }
    }
}