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
    public class MonoViewContainer : SimpleView, IViewContainer, IViewConnectionsPoint, IPoolableObject<MonoViewContainer>
    {
        public IMonoView ContainedView { get; private set; }
        [ShowInInspector] private MonoBehaviour ViewAsMono => ContainedView as MonoBehaviour;
        private IPoolElementReleaser<MonoViewContainer> _releaser;

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

        public void AssignReleaser(IPoolElementReleaser<MonoViewContainer> releaser) => _releaser = releaser;

        public void OnGet() { }

        public bool TryGetPartView(ViewPartKey key, out IView view)
        {
            if (ContainedView != null && ContainedView is IComplexMonoView complexMonoView && complexMonoView.TryGetMonoPartView(key, out var partView))
            {
                view = partView;
                return true;
            }

            view = default;
            return false;
        }

        public void AddContainedView(IMonoView monoView)
        {
            if (ContainedView != null)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError("already contain view");
#endif
                return;
            }

            ContainedView = monoView;
        }
    }
}
