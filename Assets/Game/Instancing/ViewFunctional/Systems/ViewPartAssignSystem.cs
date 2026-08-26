using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using VContainer;

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

        private Filter _filter;
        private Stash<ViewContainerComponent> _viewContainers;
        
        private readonly IViewContainersPool _viewContainersList;

        [Inject]
        public ViewPartAssignSystem(ViewSynchronizationApplier viewSynchronizationApplier, IViewContainersPool viewContainersList)
        {
            ViewSyncApplier = viewSynchronizationApplier;
            _viewContainersList = viewContainersList;
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
                    var key = Requests.Get(entity).Key;                    
                    var view = viewContainer.View;
                    if (view is IComplexMonoView complexView && complexView.TryGetPartByKey(key, out var viewPart))
                        OnPartFound(entity, viewPart);
#if UNITY_EDITOR
                    else UnityEngine.Debug.LogWarning($"view part {key.Type} not found for entity {entity.Id}");
#endif
                }

                Requests.Remove(entity);
            }
        }

        public void Dispose() { }
        protected void OnPartFound(Entity entity, IViewPart part)
        {
            ViewSyncApplier.Apply(entity, part, applyViewPosition: false);
        }
    }
}