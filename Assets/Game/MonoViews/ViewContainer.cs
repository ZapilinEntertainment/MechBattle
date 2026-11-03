using UnityEngine;
using TriInspector;
using ZE.Utils;

namespace ZE.MechBattle.Views
{
    // an object, which serves as empty container for view.
    // When entity creates, container assign immediately as his view (presenting its game world transform)
    // however its real visible view will be loaded and instances in next few frames
    // benefits:
    // - async resources load at play-time
    // - per-frame instancing can be limited
    // - no exceptions with no-view entity
    // - when visible view loads, it will be correctly synchronised to all entity components
    public class ViewContainer : SimpleView, IViewContainer, IViewConnectionsPoint, IPoolableObject<ViewContainer>
    {
        public IView ContainedView { get; private set; }
        [ShowInInspector] private MonoBehaviour ViewAsMono => ContainedView as MonoBehaviour;
        private IPoolElementReleaser<ViewContainer> _releaser;

        public void OnViewInstanced(IView instance) 
        {
            ContainedView = instance;
            ContainedView.SetParent(Transform);
        }

        protected override void OnDisposed()
        {
            _releaser.Release(this);
        }

        public void OnRelease()
        {
            if (ContainedView != null)
            {
                ContainedView.Dispose();
                ContainedView = null;
            }
        }

        public void AssignReleaser(IPoolElementReleaser<ViewContainer> releaser) => _releaser = releaser;

        public void OnGet() { }
    }
}
