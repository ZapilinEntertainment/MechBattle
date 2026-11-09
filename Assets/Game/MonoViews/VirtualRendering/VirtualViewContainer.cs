using ZE.MechBattle.Views;
using ZE.Utils;

namespace ZE.MechBattle.Views
{
    public class VirtualViewContainer : IViewContainer, IPoolableObject<VirtualViewContainer>
    {
        public IView ContainedView { get; private set; }
        private IPoolElementReleaser<VirtualViewContainer> _releaser;

        public void AssignReleaser(IPoolElementReleaser<VirtualViewContainer> releaser) =>
            _releaser = releaser;

        public void Dispose()
        {
            if (ContainedView != null)
            {
                ContainedView.Dispose();
                ContainedView = null;
            }
            _releaser.Release(this);
        }

        public void OnGet() { }

        public void OnRelease() { }

        public void OnViewInstanced(IView view) => ContainedView = view;
    }
}
