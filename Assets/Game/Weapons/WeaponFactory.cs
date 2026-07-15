using Scellecs.Morpeh;
using VContainer;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs
{
    public class WeaponFactory
    {
        private readonly World _world;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;
        private readonly StringDataDictionary _stringDictionary;
        private readonly Stash<WeaponRangeComponent> _ranges;
        private readonly Stash<DamageComponent> _damages;
        private readonly Stash<WeaponUpdateComponent> _weaponUpdateComponents;
        private readonly Stash<WeaponProjectileComponent> _weaponProjectileComponents;
        private readonly Stash<WeaponMuzzleEffectComponent> _muzzleEffects;
        private readonly Stash<WeaponTowerComponent> _weaponTowerComponents;
        private readonly Stash<WeaponBarrelComponent> _weaponBarrelComponents;
        private readonly Stash<WeaponShotPoint> _weaponShotPoints;
        private readonly Stash<WeaponAutoShotTag> _weaponAutoShotTags;
        private readonly Stash<SyncWithParentTargetTag> _syncTargetWithParent;
        private readonly Stash<RotationSpeedComponent> _rotationSpeedComponents;

        private readonly Stash<WeaponTowerViewRequestComponent> _requireTowerView;
        private readonly Stash<WeaponBarrelViewRequestComponent> _requireBarrelView;  

        [Inject]
        public WeaponFactory(World world, ParentingRelationsApplier parentingRelationsApplier, StringDataDictionary stringDataDictionary)
        {
            _world = world;
            _parentingRelationsApplier = parentingRelationsApplier;
            _stringDictionary = stringDataDictionary;

            _ranges = _world.GetStash<WeaponRangeComponent>();
            _damages = _world.GetStash<DamageComponent>();
            _weaponUpdateComponents = world.GetStash<WeaponUpdateComponent>();
            _muzzleEffects = world.GetStash<WeaponMuzzleEffectComponent>();
            _weaponProjectileComponents = world.GetStash<WeaponProjectileComponent>();

            _weaponTowerComponents = world.GetStash<WeaponTowerComponent>();
            _weaponBarrelComponents = world.GetStash<WeaponBarrelComponent>();

            _weaponShotPoints = world.GetStash<WeaponShotPoint>();
            _weaponAutoShotTags = world.GetStash<WeaponAutoShotTag>();
            _syncTargetWithParent = world.GetStash<SyncWithParentTargetTag>();

            _requireTowerView = world.GetStash<WeaponTowerViewRequestComponent>();
            _requireBarrelView = world.GetStash<WeaponBarrelViewRequestComponent>();

            _rotationSpeedComponents = world.GetStash<RotationSpeedComponent>();
        }

        public Entity CreateUnitWeapon(Entity parentEntity, WeaponConfig weaponConfig, WeaponAttachmentProtocol attachmentProtocol)
        {
            var weaponEntity = _world.CreateEntity();
            UnityEngine.Debug.Log($"built weapon with id {weaponEntity.Id}");

            _ranges.Add(weaponEntity, new(weaponConfig.MinRange, weaponConfig.MaxRange, weaponConfig.RecommendedRangePc));
            _damages.Add(weaponEntity, new() { DamageParameters = new() { Value = weaponConfig.Damage} });
            _weaponUpdateComponents.Add(weaponEntity, new(weaponConfig.Cooldown));
            _weaponAutoShotTags.Add(weaponEntity);

            _parentingRelationsApplier.Apply(new()
            {
                ParentEntity = parentEntity,
                ChildEntity = weaponEntity,
                LocalPos = attachmentProtocol.LocalPosition,
                LocalRot = attachmentProtocol.LocalRotation,
                RequestParentViewComponent = true
            });


            if (weaponConfig.TryGetProjectileId(out var projectileId)) 
                _weaponProjectileComponents.Add(weaponEntity, new(_stringDictionary.StringToKey(projectileId)));


            if (weaponConfig.TryGetMuzzleEffectId(out var muzzleEffectId))
            {
                var idKey = _stringDictionary.StringToKey(muzzleEffectId);
                var vfxKey = new VfxKey(idKey);
                _muzzleEffects.Add(weaponEntity, new(vfxKey));
            }
                
            var addTower = weaponConfig.TryGetTowerAttachmentProtocol(out var towerAttachmentProtocol);
            Entity towerEntity;
            if (addTower)
            {
                towerEntity = AttachWeaponPart(weaponEntity, towerAttachmentProtocol);
                _requireTowerView.Add(towerEntity, new(towerAttachmentProtocol.ViewPartKey));

                _weaponTowerComponents.Add(weaponEntity, new(towerEntity));
                UnityEngine.Debug.Log($"built tower with id {towerEntity.Id}");
            }
            else
            {
                towerEntity = default;
            }

            if (weaponConfig.TryGetBarrelAttachmentProtocol(out var barrelAttachmentProtocol))
            {
                var barrelEntity = AttachWeaponPart(parentEntity: addTower ? towerEntity : weaponEntity, barrelAttachmentProtocol);

                _requireBarrelView.Add(barrelEntity, new(barrelAttachmentProtocol.ViewPartKey));
                _weaponBarrelComponents.Add(weaponEntity, new(barrelEntity));

                UnityEngine.Debug.Log($"built barrel with id {barrelEntity.Id}");
            }

            _weaponShotPoints.Add(weaponEntity, new(weaponConfig.ShotPoint));

            if (weaponConfig.SyncTargetWithParent)
                _syncTargetWithParent.Add(weaponEntity);

            

            return weaponEntity;
        }

        private Entity AttachWeaponPart(Entity parentEntity,WeaponPartAttachmentProtocol protocol)
        {
            var weaponPartEntity = _world.CreateEntity();
            _parentingRelationsApplier.Apply(new()
            {
                ChildEntity = weaponPartEntity,
                ParentEntity = parentEntity,
                LocalPos = protocol.LocalPosition,
                LocalRot = quaternion.identity,
                RequestParentViewComponent = true
            });

            var rotationSpeedDegrees = protocol.RotationSpeedDegrees;
            if (rotationSpeedDegrees != 0f)
                _rotationSpeedComponents.Add(weaponPartEntity, new(math.radians(rotationSpeedDegrees)));

            _syncTargetWithParent.Add(weaponPartEntity);

            return weaponPartEntity;
        }
    }
}
