using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Views
{
    public class ComplexVirtualViewContainerHandler : ISpecifiedViewContainerHandler
    {
        public ViewMode ViewMode => ViewMode.ComplexVirtual;
        private readonly ComplexVirtualViewApplier _viewApplier;
        private readonly Stash<ViewPartKeyComponent> _viewPartKeys;
        private readonly Stash<VirtualViewComponent> _virtualViews;

        [Inject]
        public ComplexVirtualViewContainerHandler(ComplexVirtualViewApplier virtualViewApplier, World world)
        {
            _viewApplier = virtualViewApplier;
            _viewPartKeys = world.GetStash<ViewPartKeyComponent>();
            _virtualViews = world.GetStash<VirtualViewComponent>();
        }

        public void AddViewToContainer(ISpecifiedViewContainerHandler.ViewAdditionProtocol protocol)
        {
            var virtualView = protocol.View as VirtualView;
            _viewApplier.AddPartView(protocol.ViewEntity, ViewPartKey.Undefined, virtualView.ViewData, protocol.LocalPos, protocol.LocalRot, protocol.ViewContainer as VirtualViewContainer);
        }

        public bool TryGetViewPart(IViewContainer viewContainer, ViewPartKey viewPartKey, out IView viewPart)
        {
            var container = viewContainer as VirtualViewContainer;
            foreach (var partEntity in container.Parts)
            {
                var viewPartKeyComponent = _viewPartKeys.Get(partEntity, out var exists);
                if (exists && viewPartKeyComponent.Value == viewPartKey)
                {
                    viewPart = new VirtualView(partEntity, _virtualViews.Get(partEntity));
                    return true;
                }
            }

            viewPart = default;
            return false;
        }

        public void BindEntityToViewPart(Entity entity, IViewContainer viewContainer, ViewPartKey key)
        {
            throw new System.NotImplementedException();
        }
    }
}
