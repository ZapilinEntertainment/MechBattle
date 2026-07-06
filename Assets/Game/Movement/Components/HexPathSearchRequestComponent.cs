using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct HexPathSearchRequestComponent : IHexPathComponent
    {
        public readonly PortalPathDestinationKey Start;
        public readonly PortalPathDestinationKey End;
        public HexPathSearchRequestComponent(PortalPathDestinationKey start, PortalPathDestinationKey end)
        {
            Start = start;
            End = end;
        }
    }
}