using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponShotPointCalculationSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _barrelWeaponsFilter;
        private Filter _alwaysCalculateFilter;
        private Stash<WeaponShotPoint> _shotPoints;
        private Stash<WeaponBarrelComponent> _barrelComponents;
        private readonly TransformAspectHandler _transformHandler;

        [Inject]
        public WeaponShotPointCalculationSystem(TransformAspectHandler transformAspectHandler)
        {
            _transformHandler = transformAspectHandler;
        }

        public void OnAwake() 
        {
            _barrelWeaponsFilter = World.Filter
                .With<WeaponShotPoint>()
                .With<WeaponBarrelComponent>()
                .With<AttackRangeReachedTag>()
                .Build();

            _alwaysCalculateFilter = World.Filter
                .With<WeaponShotPoint>()
                .With<WeaponBarrelComponent>()
                .With<AlwaysCalculateShotPointTag>()
                .Without<AttackRangeReachedTag>()
                .Build();

            _shotPoints = World.GetStash<WeaponShotPoint>();
            _barrelComponents = World.GetStash<WeaponBarrelComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            void CalculateShotPoint(Entity entity)
            {
                ref var shotPointComponent = ref _shotPoints.Get(entity);
                var localShotPos = shotPointComponent.LocalPos;
                var barrelEntity = _barrelComponents.Get(entity).BarrelEntity;
                var barrelPoint = _transformHandler.GetPoint(barrelEntity);

                var worldShotPosition = MathExtensions.LocalToWorldPos(barrelPoint, localShotPos);
                shotPointComponent.WorldPoint = new(barrelPoint.rot, worldShotPosition);
            }

            foreach (var weaponEntity in _barrelWeaponsFilter)
            {
                CalculateShotPoint(weaponEntity);
            }

            foreach (var weaponEntity in _alwaysCalculateFilter)
            {
                CalculateShotPoint(weaponEntity);
            }
        }

        public void Dispose() { }
    }
}