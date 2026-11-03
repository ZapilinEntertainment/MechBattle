using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ExplosionCellAffectionSystem : ISystem 
    {
        // calculates and applies explosion affection on cells

        public World World { get; set;}
        private Filter _filter;
        private Stash<ExplosionParametersComponent> _explosions;
        private Stash<DamageComponent> _damage;
        private Stash<PositionComponent> _positions;
        private Stash<ExplosionCellAffectionComponent> _explosionCellAffections;
        private Stash<OwnerAffinityComponent> _casters;
        private readonly IEntitiesNavigationMap _navigationMap;

        [Inject]
        public ExplosionCellAffectionSystem(IEntitiesNavigationMap navigationMap)
        {
            _navigationMap = navigationMap;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<ExplosionCreateRequestTag>()
                .Build();

            _explosions = World.GetStash<ExplosionParametersComponent>();
            _damage = World.GetStash<DamageComponent>();
            _positions = World.GetStash<PositionComponent>();
            _explosionCellAffections = World.GetStash<ExplosionCellAffectionComponent>();
            _casters = World.GetStash<OwnerAffinityComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            var count = 0;
            foreach (var request in _filter)
            {
                var worldPos = _positions.Get(request).Value;
                var explosionParameters = _explosions.Get(request).Parameters;
                var damageParameters = _damage.Get(request).DamageParameters;
                var caster = _casters.Get(request).OwnerEntity;

                var radiusInTriangles = (int)math.ceil(explosionParameters.Radius / _navigationMap.TriangleHeight);
                var virtualHexCenter = GetClosestVertexTriposCommand.Execute(worldPos, _navigationMap.TriangleHeight);

                foreach (var tripos in new HexTrianglesEnumerator(virtualHexCenter, radiusInTriangles))
                {
                    // note: no damage spread dropdown atm

                    if (!_navigationMap.TryGetEntity(tripos, out var cellEntity))
                        continue;

                    var currentAffection = new ExplosionCellAffectionComponent(caster, damageParameters.Value);

                    var affectionComponent = _explosionCellAffections.Get(cellEntity, out var affectionAlreadyExists);
                    if (affectionAlreadyExists)
                        _explosionCellAffections.Set(cellEntity, affectionComponent + currentAffection);
                    else
                        _explosionCellAffections.Set(cellEntity, currentAffection);
                }

                World.RemoveEntity(request);
                count++;
                if (count == GameConstants.MAX_EXPLOSIONS_PER_FRAME)
                    break;
            }
        }

        public void Dispose() { }
    }
}