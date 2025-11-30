using UnityEngine;

namespace ZE.MechBattle.Views
{
    public interface IViewLoadReceiver
    {
        bool IsDisposed { get; }
        void OnViewLoaded(IView prefab);    
    }
}
