using System;

namespace ZE.MechBattle.Views
{
    public interface IViewProvider : IDisposable
    {
        bool IsReadyToProvide { get; }
        IView GetView();
    
    }
}
