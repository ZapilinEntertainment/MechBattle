using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponProjectilesCreateSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<WeaponProjectileComponent> _projectiles;
        private Stash<WeaponShotPoint> _shotPoints;
        private readonly ProjectileRequestsFactory _requestsFactory;

        [Inject]
        public WeaponProjectilesCreateSystem(ProjectileRequestsFactory projectileRequestsFactory, TransformAspectHandler transformAspectHandler)
        {
            _requestsFactory = projectileRequestsFactory;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<WeaponProjectileComponent>()
                .With<WeaponFireTag>()
                .Build();

            _projectiles = World.GetStash<WeaponProjectileComponent>();
            _shotPoints = World.GetStash<WeaponShotPoint>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            foreach (var weaponEntity in _filter)
            {
                var projectileId = _projectiles.Get(weaponEntity).ProjectileKey;
                var shotPoint = _shotPoints.Get(weaponEntity).WorldPoint;
                _requestsFactory.CreateProjectileRequestByKey(projectileId, shotPoint, weaponEntity);
            }
        }

        public void Dispose() { }

        
    }
}