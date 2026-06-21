using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.Utils;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs 
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPortalPathAccountingSystem : PathsAccountingSystemBase<HexPathIdComponent, HexPortalsPath>
    {


        public HexPortalPathAccountingSystem(HexPortalPathsLRUBuffer list) : base(list)
        {

        }

        protected override int BufferLimit => 64;

        protected override float ClearInterval => 15f;

        protected override Filter CreateFilter() => World.Filter
            .With<HexPathIdComponent>()
            .With<HexPathReadyTag>()
            .Build();
    }
}