using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ViewsDisposeSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<ViewContainerComponent> _viewContainers;
        private readonly IViewContainersPool _viewContainersPool;

        [Inject]
        public ViewsDisposeSystem(IViewContainersPool viewContainersList)
        {
            _viewContainersPool = viewContainersList;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<EntityDisposeTag>()
                .With<ViewContainerComponent>()
                .Build();

            _viewContainers = World.GetStash<ViewContainerComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var containerId = _viewContainers.Get(entity).Id;
                //UnityEngine.Debug.Log($"entity {entity.Id}, container: {containerId}, {_viewContainersPool.TryGetContainer(containerId, out _)}");
                if (!_viewContainersPool.TryGetContainer(containerId, out var viewContainer))
                    continue;

                _viewContainersPool.Release(containerId);
            }
        }

        public void Dispose() { }
    }
}