using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct DisposableViewComponent : IComponent, IDisposable 
    {
        public readonly IView View;

        public DisposableViewComponent(IView view)
        {
            View = view;
        }

        public void Dispose()
        {
            View?.Dispose();
        }

    }
}