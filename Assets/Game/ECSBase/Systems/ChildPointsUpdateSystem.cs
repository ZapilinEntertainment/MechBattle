using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ChildPointsUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _childEntities;
        private TransformAspectHandler _transformAspectHandler;
        private Stash<ParentEntityComponent> _parents;

        [Inject]
        public ChildPointsUpdateSystem(TransformAspectHandler transformAspectHandler)
        {
            _transformAspectHandler = transformAspectHandler;
        }

        public void OnAwake() 
        {
            _childEntities = World.Filter
                .With<ParentEntityComponent>()
                .With<TransformUpdatedTag>()
                .With<LocalPositionComponent>()
                .With<LocalRotationComponent>()
                .Build();

            _parents = World.GetStash<ParentEntityComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_childEntities.IsEmpty())
                return;
            foreach (var entity in _childEntities)
            {
                var parentEntity = _parents.Get(entity).Value;
                _transformAspectHandler.SyncPositionWithParent(entity, parentEntity);
            }
        }

        public void Dispose() { }
    }
}