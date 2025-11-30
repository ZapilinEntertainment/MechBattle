namespace ZE.MechBattle.Views
{
    public class PoolableView : SimpleView, IPoolableView
    {
        private IViewsPool _pool;

        public virtual void OnCreated(IViewsPool pool) { _pool = pool; }

        public virtual void OnReturnedToPool() 
        { 
            if (_pool.HostObject != null)  
                transform.parent = _pool.HostObject; 
            if (gameObject != null)
                gameObject.SetActive(false);
        }

        public virtual void OnTakenFromPool() 
        {
            transform.parent = null;
            gameObject.SetActive(true);
        }

        public override void Dispose() 
        {
            if (IsDisposed)
                return;
            _pool.ReturnElement(this);
        }
    }
}
