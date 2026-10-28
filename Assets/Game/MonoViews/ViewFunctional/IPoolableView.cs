using UnityEngine;

namespace ZE.MechBattle.Views
{
    public interface IPoolableView : IView
    {
        void OnCreated(IViewsPool pool);
        void OnTakenFromPool();
        void OnReturnedToPool();
    
    }

    public interface IViewsPool
    {
        Transform HostObject { get; }
        void ReturnElement(IPoolableView view);
    }
}
