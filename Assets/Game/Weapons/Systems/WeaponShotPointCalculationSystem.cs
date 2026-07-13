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
                .With<WeaponFireTag>()
                .With<WeaponShotPoint>()
                .With<WeaponBarrelComponent>()
                .Build();

            _shotPoints = World.GetStash<WeaponShotPoint>();
            _barrelComponents = World.GetStash<WeaponBarrelComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_barrelWeaponsFilter.IsEmpty())
                return;

            foreach (var weaponEntity in _barrelWeaponsFilter)
            {
                ref var shotPointComponent = ref _shotPoints.Get(weaponEntity);
                var localShotPos = _shotPoints.Get(weaponEntity).LocalPos;
                var barrelEntity = _barrelComponents.Get(weaponEntity).BarrelEntity;
                var barrelPoint = _transformHandler.GetPoint(barrelEntity);

                var worldShotPosition = MathExtensions.LocalToWorldPos(barrelPoint, localShotPos);
                shotPointComponent.WorldPoint = new(barrelPoint.rot, worldShotPosition);
            }
        }

        public void Dispose() { }
    }
}