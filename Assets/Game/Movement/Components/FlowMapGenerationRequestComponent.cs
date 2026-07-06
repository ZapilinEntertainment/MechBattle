using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct FlowMapGenerationRequestComponent : ITrianglePathComponent
    {
        public readonly int PortalId;    

        public FlowMapGenerationRequestComponent(int portalId) => PortalId = portalId;
    }
}