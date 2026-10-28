using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ViewPartsDisconnectionSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<ViewContainerComponent> _viewContainers;
        private readonly IViewContainersPool _viewContainersPool;
        private readonly ViewPartConnectionsList _connectionsList;

        [Inject]
        public ViewPartsDisconnectionSystem(IViewContainersPool viewContainersPool, ViewPartConnectionsList connectionsList)
        {
            _viewContainersPool = viewContainersPool;
            _connectionsList = connectionsList;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<EntityDisposeTag>()
                .With<ViewContainerComponent>()
                .With<ViewPartConnectedTag>()
                .Build();

            _viewContainers = World.GetStash<ViewContainerComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var viewContainerId = _viewContainers.Get(entity).Id;
                if (!_viewContainersPool.TryGetContainer(viewContainerId, out var viewContainer)
                    || viewContainer is not IViewConnectionsPoint viewConnectionsPoint)
                    continue;

                _connectionsList.DisconnectAll(viewConnectionsPoint);
            }
        }

        public void Dispose() { }
    }
}