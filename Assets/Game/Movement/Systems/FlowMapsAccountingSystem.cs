using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;
using ZE.Utils;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FlowMapsAccountingSystem : PathsAccountingSystemBase<FlowTrianglePathComponent, int, FlowMap>
    {
        public FlowMapsAccountingSystem(PortalFlowMapsList flowMapsList) : base(flowMapsList)
        {
        }

        protected override int BufferLimit => 64;
        protected override float ClearInterval => 20f;
    }
}