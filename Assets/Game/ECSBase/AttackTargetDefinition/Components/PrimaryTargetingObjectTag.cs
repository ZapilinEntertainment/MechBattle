using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct PrimaryTargetingObjectTag : IComponent 
    {
    // used to distinct big targets from very small (squad vs single tank, mech, single huge units)
    // used for both composite and single targets
    }
}