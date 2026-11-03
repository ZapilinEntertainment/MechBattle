using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct TargetSearchRadiusComponent : IComponent 
    {
        public readonly float MaxRadius;
        public readonly float MinRadius;
    
        public TargetSearchRadiusComponent(float maxRadius, float minRadius)
        {
            MaxRadius = maxRadius;
            MinRadius = minRadius;
        }
    }
}