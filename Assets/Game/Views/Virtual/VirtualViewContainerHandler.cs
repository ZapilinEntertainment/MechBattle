using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Views
{
    public class VirtualViewContainerHandler : ISpecifiedViewContainerHandler
    {
        public ViewMode ViewMode => ViewMode.Virtual;
        private Stash<ViewPartKeyComponent> _viewPartKeys;
        private Stash<VirtualViewComponent> _virtualViews;

        [Inject]
        public VirtualViewContainerHandler(World world)
        {
            _viewPartKeys = world.GetStash<ViewPartKeyComponent>();
            _virtualViews = world.GetStash<VirtualViewComponent>();
        }

      

        public void AddViewToContainer(ISpecifiedViewContainerHandler.ViewAdditionProtocol protocol)
        {
            // single virtual views have no containers
            throw new System.NotImplementedException();
        }

        public bool TryGetViewPart(IViewContainer viewContainer, ViewPartKey viewPartKey, out IView viewPart)
        {
            // single virtual views have no containers
            throw new System.NotImplementedException();
        }
    }
}
