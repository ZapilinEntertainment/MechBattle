using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ChildEntityAttackTargetSyncSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<AttackTargetComponent> _attackTargets;
        private Stash<ParentEntityComponent> _parentEntities;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<ParentEntityComponent>()
                .With<SyncWithParentTargetTag>()
                .Build();

            _attackTargets = World.GetStash<AttackTargetComponent>();
            _parentEntities = World.GetStash<ParentEntityComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var childEntity in _filter)
            {
                var parentEntity = _parentEntities.Get(childEntity).Value;
                var parentTargetComponent = _attackTargets.Get(parentEntity, out var parentHasTarget);
                ref var childTargetComponent = ref _attackTargets.Get(childEntity, out var childHasTarget);

                if (parentHasTarget)
                {
                    if (!childHasTarget)
                        SyncComponentsCommand.Execute<AttackTargetComponent>(childEntity, parentEntity, _attackTargets);
                    else
                        childTargetComponent.Entity = parentTargetComponent.Entity;
                }
                else
                {
                    _attackTargets.Remove(childEntity);
                }
            }

        }

        public void Dispose() { }
    }
}