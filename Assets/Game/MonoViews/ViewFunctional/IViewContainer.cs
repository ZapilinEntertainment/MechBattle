using System;

namespace ZE.MechBattle.Views
{
    public interface IViewContainer : IDisposable
    {
        IView ContainedView { get; }
        void OnViewInstanced(IView prefab);
    }
}
