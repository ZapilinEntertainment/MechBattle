using System;
using UnityEngine;

namespace ZE.MechBattle
{
    public abstract class DisposableGameObject : MonoBehaviour, IMonoView
    {
        public Transform Transform => transform;

        public bool IsDisposed { get; private set; } = false;

        public virtual void Dispose()
        {
            if (IsDisposed) return;
            OnDisposed();
        }

        public void SetParent(Transform parent) => transform.parent = parent;
        virtual protected void OnDisposed() => GameObject.Destroy(gameObject);
        private void OnDestroy()
        {
            IsDisposed = true;
        }
    }
}
