using Scellecs.Morpeh;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct TransitionHexPathComponent : IComponent 
    {
        public readonly int2 TargetHex;
        public readonly HexEdge TransitionEdge;

        public TransitionHexPathComponent(int2 targetHex, HexEdge transitionEdge)
        {
            TargetHex = targetHex;
            TransitionEdge = transitionEdge;
        }
    
    }
}