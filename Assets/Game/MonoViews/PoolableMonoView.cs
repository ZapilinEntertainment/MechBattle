using ZE.Utils;

namespace ZE.MechBattle
{
    public class PoolableMonoView : SimpleView, IPoolableObject<PoolableMonoView>
    {
        private IPoolElementReleaser<PoolableMonoView> _releaser;

        public void AssignReleaser(IPoolElementReleaser<PoolableMonoView> releaser)
        {
            _releaser = releaser;
        }

        public void OnGet() { }

        public void OnRelease() { }

        protected override void OnDisposed()
        {
            _releaser.Release(this);          
        }
    }
}
