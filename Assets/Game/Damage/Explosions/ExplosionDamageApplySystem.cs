using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ExplosionDamageApplySystem : ISystem 
    {
        public World World { get; set;}
        private Filter _damageablesFilter;
        private Filter _affectedCellsFilter;
        private Stash<ExplosionCellAffectionComponent> _cellAffections;
        private Stash<TriangularPosComponent> _triangularPositions;

        private readonly DamageApplier _damageApplier;
        private readonly IEntitiesNavigationMap _navigationMap;

        [Inject]
        public ExplosionDamageApplySystem(DamageApplier damageApplier, IEntitiesNavigationMap navigationMap)
        {
            _damageApplier = damageApplier;
            _navigationMap = navigationMap;
        }

        public void OnAwake() 
        {
            _affectedCellsFilter = World.Filter
                .With<CellEntityComponent>()
                .With<ExplosionCellAffectionComponent>()
                .Build();

            _damageablesFilter = World.Filter
                .With<HealthComponent>()
                .With<TriangularPosComponent>()
                .Build();

            _cellAffections = World.GetStash<ExplosionCellAffectionComponent>();
            _triangularPositions = World.GetStash<TriangularPosComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_affectedCellsFilter.IsEmpty())
                return;

            foreach (var entity in _damageablesFilter)
            {
                var tripos = _triangularPositions.Get(entity).Value;
                if (!_navigationMap.TryGetEntity(tripos, out var cellEntity))
                    continue;

                var affection = _cellAffections.Get(cellEntity, out var haveAffection);
                if (!haveAffection)
                    continue;

                _damageApplier.RequestDamageApply(affection.CasterEntity, entity, new(DamageType.Explosion, affection.Damage));
            }

            _cellAffections.RemoveAll();
        }

        public void Dispose() { }
    }
}