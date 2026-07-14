using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class ViewPartAssignSystem<ViewPartRequestTag, ViewLoadingTag> : ISystem  
        where ViewPartRequestTag : struct, IViewPartRequestComponent
        where ViewLoadingTag : struct, IComponent
    {
        public World World { get; set;}
        protected Stash<ViewPartRequestTag> Stash;
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
            _filter = PrepareFilter().Build();

            _viewContainers = World.GetStash<ViewContainerComponent>();
            Stash = World.GetStash<ViewPartRequestTag>();
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
                    var key = Stash.Get(entity).Key;
                    var view = viewContainer.View;
                    if (view is IComplexMonoView complexView && complexView.TryGetPartByKey(key, out var viewPart))
                        OnPartFound(entity, viewPart);
#if UNITY_EDITOR
                    else UnityEngine.Debug.LogWarning($"view part {key.Type} not found for entity {entity.Id}");
#endif
                }

                Stash.Remove(entity);
            }
        }

        public void Dispose() { }

        protected virtual FilterBuilder PrepareFilter() => World.Filter
            .With<ViewPartRequestTag>()
            .With<ViewContainerComponent>()
            .Without<ViewLoadingTag>();
        protected virtual void OnPartFound(Entity entity, IViewPart part)
        {
            ViewSyncApplier.Apply(entity, part);
        }
    }
}