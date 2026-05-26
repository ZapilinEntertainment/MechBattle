using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct RegularTrianglePathComponent : IPathUserComponent<int> 
    {
        public readonly int PathId;
        public readonly int TotalStepsCount;
        public int StepIndex;

        public int PathKey => PathId;
        
    
        public RegularTrianglePathComponent(int pathId, int length)
        {
            PathId = pathId;
            TotalStepsCount = length;
            StepIndex = 0;
        }
    }
}