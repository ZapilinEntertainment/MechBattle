using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    // allows child to get required view parts via copying ViewContainerComponent from parent to child 
    // (maybe in multiple steps, ex.: main entity -> weapon -> barrel)
    // hint: use RequestParentViewComponent option in ParentingRelationsApplier when connecting child entity to parent
    public sealed class UpdateChildViewLinkSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<AwaitingParentViewLoadingTag> _awaitTagsStash;
        private Stash<ParentEntityComponent> _parentEntities;
        private Stash<ViewLoadRequestTag> _viewLoadRequests;
        private Stash<ViewContainerComponent> _viewContainerComponents;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<AwaitingParentViewLoadingTag>()
                .With<ParentEntityComponent>()
                .Build();

            _awaitTagsStash = World.GetStash<AwaitingParentViewLoadingTag>();
            _parentEntities = World.GetStash<ParentEntityComponent>();
            _viewLoadRequests = World.GetStash<ViewLoadRequestTag>();
            _viewContainerComponents = World.GetStash<ViewContainerComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var awaitingEntity in _filter)
            {
                var parentEntity = _parentEntities.Get(awaitingEntity).Value;
                if (_viewLoadRequests.Has(parentEntity) || !_viewContainerComponents.Has(parentEntity))
                    continue;

                if (!_viewContainerComponents.Has(awaitingEntity))
                    SyncComponentsCommand.Execute<ViewContainerComponent>(awaitingEntity, parentEntity, _viewContainerComponents);
                _awaitTagsStash.Remove(awaitingEntity);
            }
        }

        public void Dispose() { }
    }
}