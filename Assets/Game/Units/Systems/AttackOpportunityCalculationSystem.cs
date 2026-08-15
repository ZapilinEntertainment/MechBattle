using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class AttackOpportunityCalculationSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Filter _clearFilter;
        private Stash<AttackOpportunintyComponent> _attackOpportunities;
        private Stash<UnitWeaponComponent> _weaponComponents;

        private Stash<AttackRangeReachedTag> _attackRangeReachedTag;
        private Stash<FireLineClearTag> _fireLineClearTag;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<UnitWeaponComponent>()
                .With<AttackTargetComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _clearFilter = World.Filter
                .With<UnitWeaponComponent>()
                .Without<AttackTargetComponent>()
                .Build();

            _attackOpportunities = World.GetStash<AttackOpportunintyComponent>();
            _weaponComponents = World.GetStash<UnitWeaponComponent>();

            _attackRangeReachedTag = World.GetStash<AttackRangeReachedTag>();
            _fireLineClearTag = World.GetStash<FireLineClearTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var weaponEntity = _weaponComponents.Get(entity).Entity;
                var attackRangeReachedCf = _attackRangeReachedTag.Has(weaponEntity) ? 1f : 0.5f;
                var fireLineClearedCf = _fireLineClearTag.Has(weaponEntity) ? 1f : 0f;
                var value = attackRangeReachedCf * fireLineClearedCf;
                //UnityEngine.Debug.Log($"entity {entity.Id} :  attackRange {attackRangeReachedCf} : fireline {fireLineClearedCf}");

                _attackOpportunities.Set(entity, new() { Value = value});
            }

            foreach (var entity in _clearFilter)
            {
                _attackOpportunities.Remove(entity);
            }
        }

        public void Dispose() { }
    }
}