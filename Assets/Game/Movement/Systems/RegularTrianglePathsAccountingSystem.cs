using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;
using ZE.Utils;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RegularTrianglePathsAccountingSystem : PathsAccountingSystemBase<RegularTrianglePathComponent, TrianglesPath>
    {
        public RegularTrianglePathsAccountingSystem(TrianglePathsLRUBuffer list) : base(list)
        {
        }

        protected override int BufferLimit => 64;

        protected override float ClearInterval => 10f;

        protected override Filter CreateFilter() => World.Filter
            .With<RegularTrianglePathComponent>()
            .With<TrianglePathReadyTag>()
            .Build();
    }
}