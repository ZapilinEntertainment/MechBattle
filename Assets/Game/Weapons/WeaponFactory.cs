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
        private readonly Stash<WeaponUpdateComponent> _weaponUpdateComponents;        
        private readonly Stash<WeaponMuzzleEffectComponent> _muzzleEffects;
        private readonly Stash<WeaponTowerComponent> _weaponTowerComponents;
        private readonly Stash<WeaponBarrelComponent> _weaponBarrelComponents;
        private readonly Stash<WeaponShotPoint> _weaponShotPoints;
        private readonly Stash<WeaponAutoShotTag> _weaponAutoShotTags;        
        private readonly Stash<RotationSpeedComponent> _rotationSpeedComponents;

        private readonly Stash<ViewPartRequestComponent> _viewPartRequests;
        private readonly Stash<WeaponTowerStowTag> _towerStowTag;
        private readonly Stash<WeaponBarrelStowTag> _barrelStowTag;

        private readonly Stash<CalculateFireLineByRaycastTag> _raycastFirelinesTag;
        private readonly Stash<LocalRotationLimitComponent> _localRotationLimits;

        private readonly Stash<DamageComponent> _damageComponents;

        private readonly Stash<WeaponProjectileComponent> _weaponProjectileComponents;
        private readonly Stash<WeaponRayComponent> _weaponRayComponents;

        private readonly Stash<ContinuosFiringTag> _continuosFiring;

        private readonly Stash<SyncWithParentTargetTag> _syncTargetWithParent;
        private readonly Stash<SyncFireTagWithParentTag> _syncFireTagWithParent;

        private readonly Stash<WeaponTag> _weaponTags;

        [Inject]
        public WeaponFactory(World world, ParentingRelationsApplier parentingRelationsApplier, StringDataDictionary stringDataDictionary)
        {
            _world = world;
            _parentingRelationsApplier = parentingRelationsApplier;
            _stringDictionary = stringDataDictionary;

            _ranges = _world.GetStash<WeaponRangeComponent>();
            _weaponUpdateComponents = world.GetStash<WeaponUpdateComponent>();
            _muzzleEffects = world.GetStash<WeaponMuzzleEffectComponent>();            

            _weaponTowerComponents = world.GetStash<WeaponTowerComponent>();
            _weaponBarrelComponents = world.GetStash<WeaponBarrelComponent>();

            _weaponShotPoints = world.GetStash<WeaponShotPoint>();
            _weaponAutoShotTags = world.GetStash<WeaponAutoShotTag>();

            _syncTargetWithParent = world.GetStash<SyncWithParentTargetTag>();
            _syncFireTagWithParent = world.GetStash<SyncFireTagWithParentTag>();

            _viewPartRequests = world.GetStash<ViewPartRequestComponent>();
            _towerStowTag = world.GetStash<WeaponTowerStowTag>();
            _barrelStowTag = world.GetStash<WeaponBarrelStowTag>();

            _raycastFirelinesTag = world.GetStash<CalculateFireLineByRaycastTag>();

            _rotationSpeedComponents = world.GetStash<RotationSpeedComponent>();
            _localRotationLimits = world.GetStash<LocalRotationLimitComponent>();

            _damageComponents = world.GetStash<DamageComponent>();
            _weaponProjectileComponents = world.GetStash<WeaponProjectileComponent>();
            _weaponRayComponents = world.GetStash<WeaponRayComponent>();

            _continuosFiring = world.GetStash<ContinuosFiringTag>();
            _weaponTags = world.GetStash<WeaponTag>();
        }

        public struct WeaponCreationProtocol
        {
            public Entity ParentEntity;
            public WeaponConfigBase WeaponConfig;
            public ViewPartAttachmentProtocol AttachmentProtocol;

            public bool UseAutoShot;
            public bool UseAutoStow;
            public bool SyncTargetWithParent;
            public bool SyncFireTagWithParent;
            public Entity ViewOwnerEntity;

            public DamageApplyParameters DamageParameters;
        }

        public Entity CreateWeapon(WeaponCreationProtocol protocol)
        {
            var weaponEntity = _world.CreateEntity();
            var weaponConfig = protocol.WeaponConfig;

            var viewOwnerEntity = protocol.ViewOwnerEntity == default ? weaponEntity : protocol.ViewOwnerEntity;

            _parentingRelationsApplier.Apply(new()
            {
                ParentEntity = protocol.ParentEntity,
                ChildEntity = weaponEntity,
                LocalPos = protocol.AttachmentProtocol.LocalPosition,
                LocalRot = protocol.AttachmentProtocol.LocalRotation,
                ViewOwnerEntity = viewOwnerEntity
            });
                
            var addTower = weaponConfig.TryGetTowerAttachmentProtocol(out var towerAttachmentProtocol);
            Entity towerEntity;
            if (addTower)
            {
                towerEntity = AttachWeaponPart(weaponEntity, viewOwnerEntity, towerAttachmentProtocol);
                _viewPartRequests.Add(towerEntity, new(towerAttachmentProtocol.ViewPartKey));

                _weaponTowerComponents.Add(weaponEntity, new(towerEntity));
                if (protocol.UseAutoStow) 
                    _towerStowTag.Add(towerEntity);

                _localRotationLimits.Set(towerEntity, new(towerAttachmentProtocol.FwdRotationLimits));

                //UnityEngine.Debug.Log($"built tower with id {towerEntity.Id}");
            }
            else
            {
                towerEntity = default;
            }

            if (weaponConfig.TryGetBarrelAttachmentProtocol(out var barrelAttachmentProtocol))
            {
                var barrelEntity = AttachWeaponPart(parentEntity: addTower ? towerEntity : weaponEntity, viewOwnerEntity, barrelAttachmentProtocol);

                _viewPartRequests.Add(barrelEntity, new(barrelAttachmentProtocol.ViewPartKey));
                _weaponBarrelComponents.Add(weaponEntity, new(barrelEntity));

                if (protocol.UseAutoStow)
                    _barrelStowTag.Add(barrelEntity);

                _localRotationLimits.Set(barrelEntity, new(barrelAttachmentProtocol.FwdRotationLimits));

                // UnityEngine.Debug.Log($"built barrel with id {barrelEntity.Id}");
            }

            ApplyWeaponComponents(weaponEntity, protocol);

            return weaponEntity;
        }

        public void ApplyWeaponComponents(Entity weaponEntity, WeaponCreationProtocol protocol)
        {
            _weaponTags.Add(weaponEntity);

            var weaponConfig = protocol.WeaponConfig;

            if (weaponConfig.TryGetMuzzleEffectId(out var muzzleEffectId))
            {
                var idKey = _stringDictionary.StringToKey(muzzleEffectId);
                var vfxKey = new VfxKey(idKey);
                _muzzleEffects.Add(weaponEntity, new(vfxKey));
            }

            _weaponShotPoints.Add(weaponEntity, new(weaponConfig.ShotPoint));

            if (protocol.SyncTargetWithParent)
                _syncTargetWithParent.Add(weaponEntity);

            if (protocol.SyncFireTagWithParent)
                _syncFireTagWithParent.Add(weaponEntity);


            SetupAttackComponents(weaponEntity, protocol);
        }

        private void SetupAttackComponents(Entity weaponEntity, WeaponCreationProtocol protocol)
        {
            var weaponConfig = protocol.WeaponConfig;

            if (weaponConfig.TryGetProjectileId(out var projectileId))
                _weaponProjectileComponents.Add(weaponEntity, new(_stringDictionary.StringToKey(projectileId)));

            if (weaponConfig.TryGetRayEffectId(out var rayEffectId))
                _weaponRayComponents.Add(weaponEntity, new(_stringDictionary.StringToKey(rayEffectId)));

            if (protocol.DamageParameters.IsValid)
                _damageComponents.Add(weaponEntity, new() { DamageParameters = protocol.DamageParameters });

            _ranges.Add(weaponEntity, new(weaponConfig.MinRange, weaponConfig.MaxRange, weaponConfig.RecommendedRangePc));
            _weaponUpdateComponents.Add(weaponEntity, new(weaponConfig.Cooldown));

            if (protocol.UseAutoShot)
            {
                _weaponAutoShotTags.Add(weaponEntity);
                _raycastFirelinesTag.Add(weaponEntity);
            }

            if (weaponConfig.ContinuousFiring)
                _continuosFiring.Add(weaponEntity);
        }

        private Entity AttachWeaponPart(Entity parentEntity, Entity viewOwnerEntity, WeaponPartAttachmentProtocol protocol)
        {
            var weaponPartEntity = _world.CreateEntity();
            _parentingRelationsApplier.Apply(new()
            {
                ChildEntity = weaponPartEntity,
                ParentEntity = parentEntity,
                LocalPos = protocol.LocalPosition,
                LocalRot = quaternion.identity,
                ViewOwnerEntity = viewOwnerEntity
            });

            var rotationSpeedDegrees = protocol.RotationSpeedDegrees;
            if (rotationSpeedDegrees != 0f)
                _rotationSpeedComponents.Add(weaponPartEntity, new(math.radians(rotationSpeedDegrees)));

            _syncTargetWithParent.Add(weaponPartEntity);

            return weaponPartEntity;
        }
    }
}
