using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class SceneViewsCleaner
    {
        private readonly Filter _filter;
        private readonly IViewContainersManager _viewContainersManager;
        private readonly Stash<ViewContainerComponent> _containerComponents;

        [Inject]
        public SceneViewsCleaner(World world, IViewContainersManager viewContainersManager)
        {
            _viewContainersManager = viewContainersManager;

            _filter = world.Filter.With<ViewContainerComponent>().Build();
            _containerComponents = world.GetStash<ViewContainerComponent>();
        }

        public void Execute()
        {
            foreach (var entity in _filter)
            {
                var containerId = _containerComponents.Get(entity).Id;
                _viewContainersManager.Release(containerId);
            }
            _containerComponents.RemoveAll();
        }
    
    }
}
