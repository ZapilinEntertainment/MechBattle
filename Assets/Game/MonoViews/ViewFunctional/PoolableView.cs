namespace ZE.MechBattle.Views
{
    public class PoolableView : SimpleView, IPoolableView
    {
        private bool _isActive = false;
        private IViewsPool _pool;

        public virtual void OnCreated(IViewsPool pool) { _pool = pool; }

        public virtual void OnReturnedToPool() 
        { 
            _isActive = false;
            if (_pool.HostObject != null)  
                transform.parent = _pool.HostObject; 
            if (gameObject != null)
                gameObject.SetActive(false);
        }

        public virtual void OnTakenFromPool() 
        {
            _isActive = true;
            transform.parent = null;
            gameObject.SetActive(true);
        }

        public override void Dispose() 
        {
            if (IsDisposed)
                return;

            if (!_isActive)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning("Attention: poolable object is already in pool");
#endif
                return;
            }
            _pool.ReturnElement(this);
        }
    }
}
