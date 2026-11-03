using UnityEngine;
using VContainer;
using Scellecs.Morpeh;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs
{
    public class ProjectilesFactory : IEntityCreationFactory
    {
        private readonly World _world;
        private readonly MonoViewFactory _viewFactory;
        private readonly StringDataDictionary _stringDict;
        private readonly IProjectileDataList _projectileData;
        private readonly TransformAspectHandler _transformAspectHandler;

        private readonly Stash<MoveSpeedComponent> _speed;
        private readonly Stash<ProjectileComponent> _projectiles;
        private readonly Stash<DamageComponent> _damage;
        private readonly Stash<ExplosionTimerComponent> _explosionTimer;
        private readonly Stash<ExplosionParametersComponent> _explosionComponents;
        private readonly Stash<OwnerAffinityComponent> _projectilesOwner;
        private readonly Stash<IgnoreUnitsCollisionByPlayermaskComponent> _ignoreCollisions;
        private readonly Stash<GravityAffectedProjectileTag> _gravityComponents;

        [Inject]
        public ProjectilesFactory(
            World world, 
            MonoViewFactory viewBuilder, 
            StringDataDictionary stringDict,
            IProjectileDataList projectileData,
            TransformAspectHandler transformAspectHandler)
        {
            _world = world;
            _viewFactory = viewBuilder;
            _stringDict = stringDict;
            _projectileData = projectileData;
            _transformAspectHandler = transformAspectHandler;
            
            _projectiles = _world.GetStash<ProjectileComponent>();
            _explosionTimer = _world.GetStash<ExplosionTimerComponent>();
            _explosionComponents = _world.GetStash<ExplosionParametersComponent>();
            _damage = _world.GetStash<DamageComponent>();
            _projectilesOwner = _world.GetStash<OwnerAffinityComponent>();
            _speed = _world.GetStash<MoveSpeedComponent>();
            _ignoreCollisions = _world.GetStash<IgnoreUnitsCollisionByPlayermaskComponent>();
            _gravityComponents = _world.GetStash<GravityAffectedProjectileTag>();
        }

      
        public Entity Build(string id, RigidTransform point, Entity weaponEntity, Entity shooterEntity) => Build(_stringDict.StringToKey(id), point, weaponEntity, shooterEntity);


        public Entity Build(int idkey, RigidTransform point, Entity weaponEntity, Entity shooterEntity)
        {
            if (_world.IsDisposed(shooterEntity) || !_projectileData.TryGetProjectileData(idkey, out var projectileData))
                return default;        

            var projectileEntity = _viewFactory.CreateViewReceiver(projectileData.ViewId);
            _transformAspectHandler.MoveToPoint(projectileEntity, point);

            _speed.Set(projectileEntity,new() { Value = projectileData.Speed});
            _explosionTimer.Set(projectileEntity, new() { Value = projectileData.Lifetime});
            _projectilesOwner.Set(projectileEntity, new(shooterEntity));
            if (projectileData.UseGravity)
                _gravityComponents.Add(projectileEntity);
             
            SetVfxExplosionComponent(projectileEntity, idkey, projectileData);
            SetExplosionComponent(projectileEntity, projectileData);

            var shooterDamageComponent = _damage.Get(weaponEntity, out var haveDamage);
            if (haveDamage) 
                _damage.Set(projectileEntity, new() { DamageParameters = shooterDamageComponent.DamageParameters });

            var ignoreCollisionComponent = _ignoreCollisions.Get(shooterEntity, out var ignoreSomePlayers);
            if (ignoreSomePlayers)
                _ignoreCollisions.Set(projectileEntity, ignoreCollisionComponent);

            return projectileEntity;
        }

        private void SetVfxExplosionComponent(Entity entity, int idkey, in ProjectileData projectileData)
        {
             var projectileComponent = new ProjectileComponent() { IdKey = idkey};
            if (!string.IsNullOrEmpty(projectileData.ExplosionEffectKey))
            {
                var vfxKey = _stringDict.StringToKey(projectileData.ExplosionEffectKey);
                 projectileComponent.ExplosionEffectKey = new VfxKey(vfxKey);                
            }
            _projectiles.Add(entity, projectileComponent);
        }

        private void SetExplosionComponent(Entity entity, in ProjectileData projectileData)
        {
            if (projectileData.ExplosionRadius == 0f)
                return;
            var explosionParameters = new ExplosionParameters()
            {
                Radius = projectileData.ExplosionRadius
            };
            _explosionComponents.Set(entity, new() { Parameters = explosionParameters });
        }
    }
}
