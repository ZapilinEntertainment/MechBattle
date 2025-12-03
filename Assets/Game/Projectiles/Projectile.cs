using System;
using UnityEngine;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class Projectile : DisposableGameObject, IViewLoadReceiver, IDisposable
    {
        private IView _view;

        protected override void OnDisposed() 
        {
            base.OnDisposed();
            _view?.Dispose();
        }

        public void OnViewLoaded(IView view) 
        {
            _view = view;
            _view.SetParent(transform);
        }
    }
}
