using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle.Views
{
    public class CompositeVirtualViewFactory : IViewInstanceStrategy
    {
        private readonly IViewContainersManager _containersManager;
        private readonly World _world;
        private readonly VirtualViewApplier _virtualViewApplier;

        [Inject]
        public CompositeVirtualViewFactory(IViewContainersManager viewContainersManager, World world)
        {
            _containersManager = viewContainersManager;
            _world = world;
        }


        Entity IViewInstanceStrategy.CreateViewEntity(string viewId)
        {
            var virtualContainer = _containersManager.GetVirtualContainer();
            _virtualViewApplier.ApplyRawViewId()

        }
    }
}
