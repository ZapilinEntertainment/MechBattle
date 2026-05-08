using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct TrianglePathProcessingComponent : IComponent 
    {
        public readonly int ProcessIndex;    
        public readonly int ProcessIteration;
        public readonly int PathId;
        public TrianglePathProcessingComponent(int processIndex, int processIteration, int pathId) 
        {
            ProcessIndex = processIndex;
            ProcessIteration = processIteration;
            PathId = pathId;
        }
    }
}