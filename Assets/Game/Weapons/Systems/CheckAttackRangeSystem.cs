using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CheckAttackRangeSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _checkFilter;
        private Filter _disableFilter;
        private Stash<WeaponRangeComponent> _ranges;
        private Stash<AttackTargetComponent> _attackTargets;
        private Stash<PositionComponent> _positions;
        private Stash<AttackRangeReachedTag> _attackRangeReachedTags;

        public void OnAwake() 
        {
            _checkFilter = World.Filter.With<WeaponRangeComponent>().With<AttackTargetComponent>().Build();
            _disableFilter = World.Filter.With<AttackRangeReachedTag>().Without<AttackTargetComponent>().Build();

            _attackTargets = World.GetStash<AttackTargetComponent>();
            _attackRangeReachedTags = World.GetStash<AttackRangeReachedTag>();
            _ranges = World.GetStash<WeaponRangeComponent>();
            _positions = World.GetStash<PositionComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _checkFilter)
            {
                var attackTarget = _attackTargets.Get(entity).Entity;
                var targetPos = _positions.Get(attackTarget).Value;
                var entityPos = _positions.Get(entity).Value;
                var distSq = math.distancesq(entityPos, targetPos);
                var isInRange = distSq < _ranges.Get(entity).MaxRangeSq;
                var hasTag = _attackRangeReachedTags.Has(entity);
                if (hasTag == isInRange)
                    continue;

                if (!hasTag)
                    _attackRangeReachedTags.Add(entity);
                else
                    _attackRangeReachedTags.Remove(entity);
            }

            foreach (var entity in _disableFilter)
            {
                _attackRangeReachedTags.Remove(entity);
            }
        }

        public void Dispose() { }
    }
}