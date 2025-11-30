using UnityEngine;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class SimpleView : MonoBehaviour, IView
    {
        protected bool IsDisposed { get;private set;} = false;

        public virtual void Dispose()
        {
            if (IsDisposed) return;
            GameObject.Destroy(gameObject);
        }

        public void SetParent(Transform parent) 
        {
            transform.SetParent(parent, false);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private void OnDestroy()
        {
            IsDisposed = true;
        }
    }
}
