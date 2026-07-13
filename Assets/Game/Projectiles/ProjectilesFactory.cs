using UnityEngine;
using VContainer;
using Scellecs.Morpeh;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs
{
    public class ProjectilesFactory
    {
        private readonly World _world;
        private readonly MonoViewFactory _viewFactory;
        private readonly StringDataDictionary _stringDict;
        private readonly ProjectilesData _projectileData;
        private readonly TransformAspectHandler _transformAspectHandler;

        private readonly Stash<MoveSpeedComponent> _speed;
        private readonly Stash<ProjectileComponent> _projectiles;
        private readonly Stash<DamageComponent> _damage;
        private readonly Stash<ExplosionTimerComponent> _explosionTimer;
        private readonly Stash<ExplosionParametersComponent> _explosionComponents;
        private readonly Stash<OwnerAffinityComponent> _projectilesOwner;

        [Inject]
        public ProjectilesFactory(
            World world, 
            MonoViewFactory viewBuilder, 
            StringDataDictionary stringDict,
            ProjectilesData projectileData,
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
        }

        public Entity Build(string id, RigidTransform point, Entity shooter) => Build(id, _stringDict.StringToKey(id), point, shooter);

        public Entity Build(int idkey, RigidTransform point, Entity shooter) => Build(_stringDict.GetStringByKey(idkey), idkey, point, shooter);

        private Entity Build(string id, int idkey, RigidTransform point, Entity shooter)
        {
            if (!_projectileData.TryGetProjectileData(id, out var projectileData))
            {
                Debug.LogError($"projectile data of {id} not found");
                return default;
            }

            var entity = _viewFactory.CreateViewReceiver(projectileData.ViewId);
            _transformAspectHandler.MoveToPoint(entity, point);

            _speed.Set(entity,new() { Value = projectileData.Speed});
            _explosionTimer.Set(entity, new() { Value = projectileData.Lifetime});
            _projectilesOwner.Set(entity, new() { OwnerEntity = shooter});
             
            SetVfxExplosionComponent(entity, idkey, projectileData);
            SetExplosionComponent(entity, projectileData);

            var shooterDamageComponent = _damage.Get(entity, out var haveDamage);
            if (haveDamage) 
                _damage.Set(entity, new() { DamageParameters = shooterDamageComponent.DamageParameters });     

            return entity;
        }

        private void SetVfxExplosionComponent(Entity entity, int idkey, in ProjectilesData.ProjectileData projectileData)
        {
             var projectileComponent = new ProjectileComponent() { IdKey = idkey};
            if (!string.IsNullOrEmpty(projectileData.ExplosionEffectKey))
            {
                var vfxKey = _stringDict.StringToKey(projectileData.ExplosionEffectKey);
                 projectileComponent.ExplosionEffectKey = new VfxKey(vfxKey);
                _projectiles.Set(entity, projectileComponent);
            }               
        }

        private void SetExplosionComponent(Entity entity, in ProjectilesData.ProjectileData projectileData)
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
