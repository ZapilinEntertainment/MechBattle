using System;
using UnityEngine;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class Projectile : MonoBehaviour, IViewLoadReceiver, IDisposable
    {
        public bool IsDisposed { get; private set; }
        private IView _view;

        public void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            _view?.Dispose();
            GameObject.Destroy(gameObject);
        }

        public void OnViewLoaded(IView view) 
        {
            _view = view;
            _view.SetParent(transform);
        }

        private void OnDestroy()
        {
            IsDisposed = true;
        }
    }
}
