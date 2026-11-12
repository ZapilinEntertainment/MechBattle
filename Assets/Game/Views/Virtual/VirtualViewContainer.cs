using System.Collections.Generic;
using ZE.Utils;

namespace ZE.MechBattle.Views
{
    public class VirtualViewContainer : IViewContainer, IPoolableObject<VirtualViewContainer>
    {
        private IPoolElementReleaser<VirtualViewContainer> _releaser;

        public void AssignReleaser(IPoolElementReleaser<VirtualViewContainer> releaser) =>
            _releaser = releaser;

        public void Dispose()
        {
            _releaser.Release(this);
        }

        public void OnGet() { }

        public void OnRelease() 
        {
            
        }
    }
}
