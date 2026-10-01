using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

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

        private const float RAYCAST_VALUE_RAISE_SPEED = 5f;
        private const float RAYCAST_VALUE_FALL_SPEED = 0.1f;

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
                ref var attackOpportunitiesComponent = ref _attackOpportunities.Get(entity, out var attackOpportunityExists);
                if (!attackOpportunityExists)
                {
                    _attackOpportunities.Add(entity);
                    attackOpportunitiesComponent = ref _attackOpportunities.Get(entity);
                }

                attackOpportunitiesComponent.RangeValue = _attackRangeReachedTag.Has(weaponEntity) ? 1f : 0f;
                //todo: calculate exact distance cf


                var newFireLineValue = _fireLineClearTag.Has(weaponEntity) ? 1f : 0f;

                var currentFirelineValue = attackOpportunitiesComponent.FireLineValue;
                var comparationResult = newFireLineValue.CompareTo(currentFirelineValue);
                if (comparationResult != 0)
                {
                    attackOpportunitiesComponent.FireLineValue = MathExtensions.MoveTowards(
                        attackOpportunitiesComponent.FireLineValue, 
                        newFireLineValue, 
                        comparationResult == 1 ? RAYCAST_VALUE_RAISE_SPEED *deltaTime : RAYCAST_VALUE_FALL_SPEED * deltaTime);
                }

                attackOpportunitiesComponent.ResultingValue = attackOpportunitiesComponent.RangeValue * attackOpportunitiesComponent.FireLineValue;
            }

            foreach (var entity in _clearFilter)
            {
                _attackOpportunities.Remove(entity);
            }
        }

        public void Dispose() { }
    }
}