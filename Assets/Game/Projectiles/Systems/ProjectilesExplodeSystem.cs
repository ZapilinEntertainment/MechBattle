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
        private Stash<DamageComponent> _damageComponents;
        private Stash<ExplosionParametersComponent> _explosionParametersComponents;
        private Stash<CollisionComponent> _collisionComponents;
        private Stash<ProjectileComponent> _projectileComponents;
        private Stash<OwnerAffinityComponent> _ownerComponents;
        private Stash<EntityDisposeTag> _entityDisposeTags;
        private Stash<PositionComponent> _positionComponents;
        private Stash<RotationComponent> _rotationComponents;

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

            _damageComponents = World.GetStash<DamageComponent>();
            _explosionParametersComponents = World.GetStash<ExplosionParametersComponent>();
            _collisionComponents = World.GetStash<CollisionComponent>();
            _projectileComponents = World.GetStash<ProjectileComponent>();
            _ownerComponents = World.GetStash<OwnerAffinityComponent>();
            _entityDisposeTags = World.GetStash<EntityDisposeTag>();

            _positionComponents = World.GetStash<PositionComponent>();
            _rotationComponents = World.GetStash<RotationComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_selfExplodedProjectiles.IsNotEmpty())
            {
                foreach (var projectile in _selfExplodedProjectiles)
                {
                    var position = _positionComponents.Get(projectile).Value;
                    var rotationComponent = _rotationComponents.Get(projectile, out var rotationPresented).Value;

                    CreateVfxExplosion(projectile, position, rotationPresented ? rotationComponent.value : UnityEngine.Random.rotation);
                    var explosionComponent = _explosionParametersComponents.Get(projectile, out var isExplosible);
                    if (isExplosible)
                        CreateDamagingExplosion(projectile, explosionComponent.Parameters);

                    _entityDisposeTags.Add(projectile);
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
                        //else UnityEngine.Debug.Log("no collider defined: " + collisionResult.HitColliderId.ToString());
                    }

                    var position = _positionComponents.Get(projectile).Value;
                    CreateVfxExplosion(projectile, position, quaternion.LookRotation(collisionResult.HitNormal, math.up()));

                    _entityDisposeTags.Add(projectile);
                }
            }
        }

        public void Dispose() { }

        private void CreateVfxExplosion(Entity projectile, float3 position, quaternion rotation) 
        {
            var vfxExplosionKey = _projectileComponents.Get(projectile).ExplosionEffectKey;
            if (vfxExplosionKey.IsDefined)
                _vfxRequestsBuilder.Build(vfxExplosionKey, position, rotation);
        }

        private void CreateDamagingExplosion(Entity projectile, ExplosionParameters parameters) 
        {
            var position = _positionComponents.Get(projectile).Value;
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
                //UnityEngine.Debug.Log("request damage: " + damageComponent.DamageParameters.Value.ToString());
            }
        }
    }
}