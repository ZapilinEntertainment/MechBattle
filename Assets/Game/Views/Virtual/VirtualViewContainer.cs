using Scellecs.Morpeh;
using System.Collections.Generic;
using ZE.Utils;

namespace ZE.MechBattle.Views
{
    public class VirtualViewContainer : IViewContainer, IPoolableObject<VirtualViewContainer>
    {
        public IReadOnlyList<Entity> Parts => _parts;

        public ViewMode ViewMode => ViewMode.Virtual;

        private IPoolElementReleaser<VirtualViewContainer> _releaser;
        private List<Entity> _parts = new(4);

        public void AssignReleaser(IPoolElementReleaser<VirtualViewContainer> releaser) =>
            _releaser = releaser;

        public void Dispose()
        {
            _releaser.Release(this);
        }

        public void OnGet() { }

        public void OnRelease() { _parts.Clear(); }


        public void AddPartEntity(Entity entity) => _parts.Add(entity);
    }
}
