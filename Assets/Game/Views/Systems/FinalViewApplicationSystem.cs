using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FinalViewApplicationSystem : TagsClearSystem<FinalViewApplicationRequiredTag> 
    {
        private Filter _filter;
        private Stash<ViewContainerComponent> _viewContainers;
        private readonly IViewContainerHandler _viewContainerHandler;

        [Inject]
        public FinalViewApplicationSystem(IViewContainerHandler viewContainerHandler)
        {
            _viewContainerHandler = viewContainerHandler;
        }

        public override void OnAwake()
        {
            base.OnAwake();
            _filter = World.Filter
                .With<FinalViewApplicationRequiredTag>()
                .With<ViewContainerComponent>()
                .Build();

            _viewContainers = World.GetStash<ViewContainerComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var entity in _filter)
            {
                var viewContainerId = _viewContainers.Get(entity).Id;
                _viewContainerHandler.OnFinalViewApplied(entity, viewContainerId);
            }

            base.OnUpdate(deltaTime);
        }
        
    }
}