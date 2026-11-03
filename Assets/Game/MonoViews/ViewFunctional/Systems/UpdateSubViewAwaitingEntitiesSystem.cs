using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    // allows child to get required view parts via copying ViewContainerComponent from parent to child 
    // (maybe in multiple steps, ex.: main entity -> weapon -> barrel)
    // hint: use RequestParentViewComponent option in ParentingRelationsApplier when connecting child entity to parent
    public sealed class UpdateSubViewAwaitingEntitiesSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<AwaitingViewLoadingComponent> _awaitingViewComponents;
        private Stash<ViewLoadRequestTag> _viewLoadRequests;
        private Stash<ViewContainerComponent> _viewContainerComponents;
        private Stash<ViewPartRequestComponent> _viewPartRequests;

        private readonly EntityViewHandler _viewHandler;

        [Inject]
        public UpdateSubViewAwaitingEntitiesSystem(EntityViewHandler viewHandler)
        {
            _viewHandler = viewHandler;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<AwaitingViewLoadingComponent>()
                .With<ParentEntityComponent>()
                .Build();

            _awaitingViewComponents = World.GetStash<AwaitingViewLoadingComponent>();
            _viewLoadRequests = World.GetStash<ViewLoadRequestTag>();
            _viewContainerComponents = World.GetStash<ViewContainerComponent>();
            _viewPartRequests = World.GetStash<ViewPartRequestComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var subViewEntity in _filter)
            {
                var viewOwner = _awaitingViewComponents.Get(subViewEntity).ViewOwnerEntity;
                var viewOwnerContainerComponent = _viewContainerComponents.Get(viewOwner, out var ownerHaveViewContainer);
                if (_viewLoadRequests.Has(viewOwner) || !ownerHaveViewContainer)
                    continue;

                if (!_viewContainerComponents.Has(subViewEntity))
                {
                    // just set owners view container to entity,
                    // and assign system will check its view for required view part
                    SyncComponentsCommand.Execute<ViewContainerComponent>(subViewEntity, viewOwner, _viewContainerComponents);
                }                    
                else
                {
                    var viewKey = _viewPartRequests.Get(subViewEntity, out var haveViewRequest).Key;
                    if (haveViewRequest)
                    {
                        // if subview have its own view container
                        // transfer view part authority to this entity (mono-view will also change parent)
                        _viewHandler.TransferPartAuthority(viewOwnerContainerComponent.Id, subViewEntity, viewKey);
                        _viewPartRequests.Remove(subViewEntity);
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"entity {subViewEntity.Id} has await view component, but no request component");
                    }
                }
                _awaitingViewComponents.Remove(subViewEntity);
            }
        }

        public void Dispose() { }
    }
}