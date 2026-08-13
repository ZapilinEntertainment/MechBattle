using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class EntityViewHandler
    {
        private readonly Stash<ViewContainerComponent> _viewContainers;
        private readonly ViewContainersPool _viewContainersList;

        [Inject]
        public EntityViewHandler(World world,ViewContainersPool viewContainersList )
        {
            _viewContainersList = viewContainersList;
            _viewContainers = world.GetStash<ViewContainerComponent>();
        }

        public bool TryGetEntityView<T>(Entity entity, out T view) where T : class, IView
        {
            view = default;
            var viewContainerComponent = _viewContainers.Get(entity, out var viewContainerExists);           
            if (!viewContainerExists || !_viewContainersList.TryGetContainer(viewContainerComponent.Id, out var viewContainer))
                return false;

            view = viewContainer.View as T;            
            return view != null;
        }
    
    }
}
