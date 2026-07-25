using System.Collections.Generic;
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
        private Stash<ChildTransformLastSyncStampComponent> _stampComponents;
        private int _iterationNumber = 1;
        private readonly HashSet<Entity> _operatedEntities = new();

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
            _stampComponents = World.GetStash<ChildTransformLastSyncStampComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_childEntities.IsEmpty())
                return;
            foreach (var entity in _childEntities)
            {
                if (_operatedEntities.Contains(entity))
                    continue;
                CheckEntityHierarchyUp(entity);
            }

            _operatedEntities.Clear();
            _iterationNumber++;
        }

        public void Dispose() { }

        private void CheckEntityHierarchyUp(Entity entity)
        {
            // adding to list at start, to prevent endless cycle
            _operatedEntities.Add(entity);
            var parentComponent = _parents.Get(entity, out var parentExists);

            if (!parentExists)
                return;

            if (!_operatedEntities.Contains(parentComponent.Value))
                CheckEntityHierarchyUp(parentComponent.Value);

            var stampIterationComponent = _stampComponents.Get(entity, out var stampExists);
            if (!stampExists || stampIterationComponent.LastSyncIteration != _iterationNumber)
            {
                _transformAspectHandler.SyncPositionWithParent(entity, parentComponent.Value);
                _stampComponents.Set(entity, new() { LastSyncIteration = _iterationNumber });
            }                
        }
    }
}