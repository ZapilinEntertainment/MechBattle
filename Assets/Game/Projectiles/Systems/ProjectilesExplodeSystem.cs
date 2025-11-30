using VContainer;
using Unity.Mathematics;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectilesExplodeSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _selfExplodedProjectiles;
        private Filter _collidedProjectiles;
        private Stash<TransformComponent> _transforms;
        private Stash<DamageComponent> _damageComponents;
        private Stash<ExplosionParametersComponent> _explosionParametersComponents;
        private Stash<CollisionComponent> _collisionComponents;
        private Stash<ProjectileComponent> _projectileComponents;
        private Stash<OwnerAffinityComponent> _ownerComponents;

        private readonly CollidersTable _collidersTable;
        private readonly VfxRequestsBuilder _vfxRequestsBuilder;
        private readonly ExplosionRequestsBuilder _explosionRequestsBuilder;
        private readonly DamageRequestsBuilder _damageRequestsBuilder;

        [Inject]
        public ProjectilesExplodeSystem(
            CollidersTable collidersTable, 
            VfxRequestsBuilder vfxRequestsBuilder, 
            ExplosionRequestsBuilder explosionRequestsBuilder,
            DamageRequestsBuilder damageRequestsBuilder)
        {
            _collidersTable = collidersTable;
            _vfxRequestsBuilder = vfxRequestsBuilder;
            _explosionRequestsBuilder = explosionRequestsBuilder;
            _damageRequestsBuilder = damageRequestsBuilder;
        }

        public void OnAwake() 
        {
            _selfExplodedProjectiles = World.Filter
                .With<ProjectileComponent>()
                .With<ExplodeTag>()
                .Without<CollisionComponent>()
                .Build();

            _collidedProjectiles = World.Filter
                .With<ProjectileComponent>()
                .With<ExplodeTag>()
                .With<CollisionComponent>()
                .Build();

            _transforms = World.GetStash<TransformComponent>();
            _damageComponents = World.GetStash<DamageComponent>();
            _explosionParametersComponents = World.GetStash<ExplosionParametersComponent>();
            _collisionComponents = World.GetStash<CollisionComponent>();
            _projectileComponents = World.GetStash<ProjectileComponent>();
            _ownerComponents = World.GetStash<OwnerAffinityComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_selfExplodedProjectiles.IsNotEmpty())
            {
                foreach (var projectile in _selfExplodedProjectiles)
                {
                    var transform = _transforms.Get(projectile);
                    CreateVfxExplosion(projectile, transform.Position, transform.Rotation);
                    var explosionComponent = _explosionParametersComponents.Get(projectile, out var isExplosible);
                    if (isExplosible)
                        CreateDamagingExplosion(projectile, explosionComponent.Parameters);

                    World.RemoveEntity(projectile);
                }
            }

            if (_collidedProjectiles.IsNotEmpty())
            {
                foreach (var projectile in _collidedProjectiles)
                {               
                    var explosionComponent = _explosionParametersComponents.Get(projectile, out var isExplosible);
                    var collisionResult = _collisionComponents.Get(projectile).Result;

                    if (isExplosible)
                    {
                        CreateDamagingExplosion(projectile, explosionComponent.Parameters);
                    }                        
                    else
                    {                        
                        if (_collidersTable.TryGetColliderOwner(collisionResult.HitColliderId, out var targetEntity))
                            RequestDirectDamage(projectile, targetEntity);
                    }

                    var transform = _transforms.Get(projectile);
                    CreateVfxExplosion(projectile, transform.Position, quaternion.LookRotation(collisionResult.HitNormal, math.up()));

                    World.RemoveEntity(projectile);
                }
            }
        }

        public void Dispose() { }

        private void CreateVfxExplosion(Entity projectile, float3 position, quaternion rotation) 
        {
            var vfxExplosionKey = _projectileComponents.Get(projectile).ExplosionEffectKey;
            if (vfxExplosionKey.IsDefined)
            {
                var transform = _transforms.Get(projectile);
                _vfxRequestsBuilder.Build(vfxExplosionKey, transform.Position, transform.Rotation);
            }
        }

        private void CreateDamagingExplosion(Entity projectile, ExplosionParameters parameters) 
        {
            var position = _transforms.Get(projectile).Position;
            var damage = _damageComponents.Get(projectile, out var isDamagingProjectile);
            if (isDamagingProjectile)
                _explosionRequestsBuilder.RequestExplosion(position, parameters, damage.DamageParameters);
        }

        private void RequestDirectDamage(Entity projectile, Entity target) 
        {
            var damageComponent = _damageComponents.Get(projectile, out var isDamagingProjectile);
            if (isDamagingProjectile)
            {
                var projectileOwnerComponent = _ownerComponents.Get(projectile, out var projectileOwnerExists);
                _damageRequestsBuilder.Build(
                    damager: projectileOwnerExists ? projectileOwnerComponent.OwnerEntity : default,
                    target: target,
                    damageParameters: damageComponent.DamageParameters);
            }
        }
    }
}