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
        private readonly Stash<WeaponProjectileComponent> _weaponProjectileComponents;
        private readonly Stash<WeaponMuzzleEffectComponent> _muzzleEffects;
        private readonly Stash<WeaponTowerComponent> _weaponTowerComponents;
        private readonly Stash<WeaponBarrelComponent> _weaponBarrelComponents;
        private readonly Stash<WeaponShotPoint> _weaponShotPoints;
        private readonly Stash<WeaponAutoShotTag> _weaponAutoShotTags;
        private readonly Stash<SyncWithParentTargetTag> _syncTargetWithParent;
        private readonly Stash<RotationSpeedComponent> _rotationSpeedComponents;

        private readonly Stash<ViewPartRequestComponent> _viewPartRequests;
        private readonly Stash<WeaponTowerStowTag> _towerStowTag;
        private readonly Stash<WeaponBarrelStowTag> _barrelStowTag;

        private readonly Stash<CalculateFireLineByRaycastTag> _raycastFirelinesTag;
        private readonly Stash<LocalRotationLimitComponent> _localRotationLimits;

        private readonly Stash<DamageComponent> _damageComponents;

        [Inject]
        public WeaponFactory(World world, ParentingRelationsApplier parentingRelationsApplier, StringDataDictionary stringDataDictionary)
        {
            _world = world;
            _parentingRelationsApplier = parentingRelationsApplier;
            _stringDictionary = stringDataDictionary;

            _ranges = _world.GetStash<WeaponRangeComponent>();
            _weaponUpdateComponents = world.GetStash<WeaponUpdateComponent>();
            _muzzleEffects = world.GetStash<WeaponMuzzleEffectComponent>();
            _weaponProjectileComponents = world.GetStash<WeaponProjectileComponent>();

            _weaponTowerComponents = world.GetStash<WeaponTowerComponent>();
            _weaponBarrelComponents = world.GetStash<WeaponBarrelComponent>();

            _weaponShotPoints = world.GetStash<WeaponShotPoint>();
            _weaponAutoShotTags = world.GetStash<WeaponAutoShotTag>();
            _syncTargetWithParent = world.GetStash<SyncWithParentTargetTag>();

            _viewPartRequests = world.GetStash<ViewPartRequestComponent>();
            _towerStowTag = world.GetStash<WeaponTowerStowTag>();
            _barrelStowTag = world.GetStash<WeaponBarrelStowTag>();

            _raycastFirelinesTag = world.GetStash<CalculateFireLineByRaycastTag>();

            _rotationSpeedComponents = world.GetStash<RotationSpeedComponent>();
            _localRotationLimits = world.GetStash<LocalRotationLimitComponent>();

            _damageComponents = world.GetStash<DamageComponent>();
        }

        public struct WeaponCreationProtocol
        {
            public Entity ParentEntity;
            public WeaponConfig WeaponConfig;
            public ViewPartAttachmentProtocol AttachmentProtocol;

            public bool UseAutoShot;
            public bool UseAutoStow;
            public bool SyncTargetWithParent;

            public DamageApplyParameters DamageParameters;
        }

        public Entity CreateWeapon(WeaponCreationProtocol protocol)
        {
            var weaponEntity = _world.CreateEntity();
            var weaponConfig = protocol.WeaponConfig;

            _ranges.Add(weaponEntity, new(weaponConfig.MinRange, weaponConfig.MaxRange, weaponConfig.RecommendedRangePc));
            _weaponUpdateComponents.Add(weaponEntity, new(weaponConfig.Cooldown));
            if (protocol.UseAutoShot)
            {
                _weaponAutoShotTags.Add(weaponEntity);
                _raycastFirelinesTag.Add(weaponEntity);
            }            

            _parentingRelationsApplier.Apply(new()
            {
                ParentEntity = protocol.ParentEntity,
                ChildEntity = weaponEntity,
                LocalPos = protocol.AttachmentProtocol.LocalPosition,
                LocalRot = protocol.AttachmentProtocol.LocalRotation,
                AwaitParentViewComponent = true
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
                var barrelEntity = AttachWeaponPart(parentEntity: addTower ? towerEntity : weaponEntity, barrelAttachmentProtocol);

                _viewPartRequests.Add(barrelEntity, new(barrelAttachmentProtocol.ViewPartKey));
                _weaponBarrelComponents.Add(weaponEntity, new(barrelEntity));

                if (protocol.UseAutoStow)
                    _barrelStowTag.Add(barrelEntity);

                _localRotationLimits.Set(barrelEntity, new(barrelAttachmentProtocol.FwdRotationLimits));

                // UnityEngine.Debug.Log($"built barrel with id {barrelEntity.Id}");
            }

            _weaponShotPoints.Add(weaponEntity, new(weaponConfig.ShotPoint));

            if (protocol.SyncTargetWithParent)
                _syncTargetWithParent.Add(weaponEntity);

            if (protocol.DamageParameters.IsValid)
                _damageComponents.Add(weaponEntity, new() { DamageParameters = protocol.DamageParameters });

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
                AwaitParentViewComponent = true
            });

            var rotationSpeedDegrees = protocol.RotationSpeedDegrees;
            if (rotationSpeedDegrees != 0f)
                _rotationSpeedComponents.Add(weaponPartEntity, new(math.radians(rotationSpeedDegrees)));

            _syncTargetWithParent.Add(weaponPartEntity);

            return weaponPartEntity;
        }
    }
}
