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
        private Stash<WeaponTargetPositionComponent> _weaponTargetPositions;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<ParentEntityComponent>()
                .With<SyncWithParentTargetTag>()
                .Build();

            _attackTargets = World.GetStash<AttackTargetComponent>();
            _parentEntities = World.GetStash<ParentEntityComponent>();
            _weaponTargetPositions = World.GetStash<WeaponTargetPositionComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var childEntity in _filter)
            {
                var parentEntity = _parentEntities.Get(childEntity).Value;
                Sync(childEntity, parentEntity, _attackTargets);
                Sync(childEntity, parentEntity, _weaponTargetPositions);
            }

        }

        public void Dispose() { }

        private void Sync<T>(Entity childEntity, Entity parentEntity, Stash<T> stash ) where T : struct, IComponent
        {
            
            var parentComponent = stash.Get(parentEntity, out var parentHasComponent);

            if (parentHasComponent)
            {
                if (!stash.Has(childEntity))
                    SyncComponentsCommand.Execute<T>(childEntity, parentEntity, stash);
                else
                    stash.Set(childEntity, parentComponent);
            }
            else
            {
                _attackTargets.Remove(childEntity);
            }
        }
    }
}