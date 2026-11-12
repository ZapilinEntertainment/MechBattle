using System.Collections.Generic;
using ZE.Utils;
using static ZE.MechBattle.CompositeVirtualViewConfiguration;

namespace ZE.MechBattle.Views
{
    public class VirtualViewContainer : IViewContainer, IPoolableObject<VirtualViewContainer>
    {
        private IPoolElementReleaser<VirtualViewContainer> _releaser;
        private List<VirtualPartConfig> _configs = new(4);

        public void AssignReleaser(IPoolElementReleaser<VirtualViewContainer> releaser) =>
            _releaser = releaser;

        public void Dispose()
        {
            _releaser.Release(this);
        }

        public void OnGet() { }

        public void OnRelease() 
        {
            _configs.Clear();
        }

        public bool TryGetPartView(ViewPartKey key, out IView view)
        {
            foreach (var config in _configs)
            {
                if (config.PartKey == key)
                {
                    view = new VirtualView(config.ViewData);
                    return true;
                }
            }

            view = default;
            return false;
        }

        public void AddContainedView(IView view)
        {
            if (view is IVirtualView virtualView)
                _configs.Add(new(virtualView.ViewData));
            else
                UnityEngine.Debug.LogError("wrong type of view");
        }

        public void AddPartView(VirtualPartConfig config) => _configs.Add(config);
    }
}
