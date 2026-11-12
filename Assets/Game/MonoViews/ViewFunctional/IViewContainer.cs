using System;

namespace ZE.MechBattle.Views
{
    public interface IViewContainer : IDisposable
    {
        bool TryGetPartView(ViewPartKey key, out IView view);
        void AddContainedView(IView view);
    }
}
