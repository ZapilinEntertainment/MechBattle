using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct LocalRotationLimitComponent : IComponent 
    {
        public readonly ForwardRotationLimits DotLimits;    

        public LocalRotationLimitComponent(ForwardRotationLimits dotLimits)
        {
            DotLimits = dotLimits;
        }
    }
}