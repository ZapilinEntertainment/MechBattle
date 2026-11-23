using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Views;

namespace ZE.MechBattle.Ecs {

    // this systems assign discrete parts of view to exact entities (ex.: tank barrel \ tower). 

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class ViewPartAssignSystem : ISystem  
    {
        public World World { get; set;}
        protected Stash<ViewPartRequestComponent> Requests;
        protected readonly ViewSynchronizationApplier ViewSyncApplier;
        private readonly DelayedMonoViewContainerHandler _delayedMonoContainerHandler;

        private Filter _filter;
        private Stash<ViewContainerComponent> _viewContainers;
        
        private readonly IViewContainersManager _viewContainersList;

        [Inject]
        public ViewPartAssignSystem(
            ViewSynchronizationApplier viewSynchronizationApplier, 
            IViewContainersManager viewContainersList,
            DelayedMonoViewContainerHandler delayedMonoViewContainerHandler)
        {
            ViewSyncApplier = viewSynchronizationApplier;
            _viewContainersList = viewContainersList;
            _delayedMonoContainerHandler = delayedMonoViewContainerHandler;
        }

        public virtual void OnAwake() 
        {
            _filter = World.Filter
            .With<ViewPartRequestComponent>()
            .With<ViewContainerComponent>()
            .Without<AwaitingViewLoadingComponent>()
            .Build();

            _viewContainers = World.GetStash<ViewContainerComponent>();
            Requests = World.GetStash<ViewPartRequestComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            foreach (var entity in _filter)
            {
                var viewContainerComponent = _viewContainers.Get(entity);
                if (!_viewContainersList.TryGetContainer(viewContainerComponent.Id, out var viewContainer))
                {
                    #if UNITY_EDITOR
                    UnityEngine.Debug.LogWarning($"container {viewContainerComponent.Id} not found for entity {entity.Id}");
                    #endif
                }
                else
                {
                    _delayedMonoContainerHandler.BindEntityToViewPart(entity, viewContainer as MonoViewContainer, Requests.Get(entity).Key);
                }

                Requests.Remove(entity);
            }
        }

        public void Dispose() { }
    }
}