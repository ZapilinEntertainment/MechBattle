using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct TrianglePathProcessingComponent : IComponent 
    {
        public readonly int PathId;

        public TrianglePathProcessingComponent(int pathId) 
        {
            PathId = pathId;
        }
    }
}