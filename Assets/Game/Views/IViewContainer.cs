using System;

namespace ZE.MechBattle.Views
{
    public interface IViewContainer : IDisposable
    {
        bool TryGetContainedViewAs<T>(out T view) where T : IView;
    }
}
