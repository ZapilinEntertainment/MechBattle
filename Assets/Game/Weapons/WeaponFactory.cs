using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Weapons;

namespace ZE.MechBattle
{
    public class WeaponFactory
    {
        private readonly World _world;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;
        private readonly StringDataDictionary _stringDictionary;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly WeaponHandler _weaponHandler;

        private readonly List<IWeaponFeatureApplier> _featureAppliers;

        private readonly Stash<WeaponRangeComponent> _ranges;
        private readonly Stash<WeaponLoadingComponent> _weaponUpdateComponents;        
        private readonly Stash<WeaponMuzzleEffectComponent> _muzzleEffects;
        private readonly Stash<WeaponTowerComponent> _weaponTowerComponents;
        private readonly Stash<WeaponBarrelComponent> _weaponBarrelComponents;
        private readonly Stash<WeaponShotPoint> _weaponShotPoints;        
        private readonly Stash<RotationSpeedComponent> _rotationSpeedComponents;

        private readonly Stash<ViewPartRequestComponent> _viewPartRequests;
        private readonly Stash<WeaponTowerStowTag> _towerStowTag;
        private readonly Stash<WeaponBarrelStowTag> _barrelStowTag;
     
        private readonly Stash<LocalRotationLimitComponent> _localRotationLimits;

        private readonly Stash<DamageComponent> _damageComponents;

        private readonly Stash<WeaponProjectileComponent> _weaponProjectileComponents;
        private readonly Stash<WeaponRayComponent> _weaponRayComponents;

        private readonly Stash<ContinuosFiringTag> _continuosFiringTag;

        private readonly Stash<SyncWithParentTargetTag> _syncTargetWithParent;
        private readonly Stash<SyncFireTagWithParentTag> _syncFireTagWithParent;

        private readonly Stash<WeaponComponent> _weaponComponents;

        private readonly Stash<AlwaysCalculateShotPointTag> _alwaysCalculateShotPointsTag;

        [Inject]
        public WeaponFactory(
            World world, 
            ParentingRelationsApplier parentingRelationsApplier, 
            StringDataDictionary stringDataDictionary,
            TransformAspectHandler transformAspectHandler,
            WeaponHandler weaponHandler,
            IEnumerable<IWeaponFeatureApplier> featureAppliers)
        {
            _world = world;
            _parentingRelationsApplier = parentingRelationsApplier;
            _stringDictionary = stringDataDictionary;
            _transformAspectHandler = transformAspectHandler;
            _weaponHandler = weaponHandler;

            if (featureAppliers != null)
            {
                _featureAppliers = new(8);
                foreach (var featureApplier in featureAppliers)
                {
                    _featureAppliers.Add(featureApplier);
                }
                _featureAppliers.Sort();
            }
            else
            {
                _featureAppliers = new(0);
            }
           

            _ranges = _world.GetStash<WeaponRangeComponent>();
            _weaponUpdateComponents = world.GetStash<WeaponLoadingComponent>();
            _muzzleEffects = world.GetStash<WeaponMuzzleEffectComponent>();            

            _weaponTowerComponents = world.GetStash<WeaponTowerComponent>();
            _weaponBarrelComponents = world.GetStash<WeaponBarrelComponent>();

            _weaponShotPoints = world.GetStash<WeaponShotPoint>();

            _syncTargetWithParent = world.GetStash<SyncWithParentTargetTag>();
            _syncFireTagWithParent = world.GetStash<SyncFireTagWithParentTag>();

            _viewPartRequests = world.GetStash<ViewPartRequestComponent>();
            _towerStowTag = world.GetStash<WeaponTowerStowTag>();
            _barrelStowTag = world.GetStash<WeaponBarrelStowTag>();

            _rotationSpeedComponents = world.GetStash<RotationSpeedComponent>();
            _localRotationLimits = world.GetStash<LocalRotationLimitComponent>();

            _damageComponents = world.GetStash<DamageComponent>();
            _weaponProjectileComponents = world.GetStash<WeaponProjectileComponent>();
            _weaponRayComponents = world.GetStash<WeaponRayComponent>();

            _continuosFiringTag = world.GetStash<ContinuosFiringTag>();
            _weaponComponents = world.GetStash<WeaponComponent>();

            _alwaysCalculateShotPointsTag = world.GetStash<AlwaysCalculateShotPointTag>();
        }

        public struct WeaponCreationProtocol
        {
            public Entity ParentEntity;
            public Entity WeaponControllerEntity;
            public IWeaponConfig WeaponConfig;
            public ViewPartAttachmentProtocol WeaponAttachmentProtocol;

            public bool SyncTargetWithParent;
            public bool SyncFireTagWithParent;
            public bool AlwaysCalculateShotPoint;
            public bool UseAutoShot;
            public bool UseAutoStow;
            public Entity ViewOwnerEntity;

            public DamageApplyParameters DamageParameters;
        }

        public Entity CreateWeapon(WeaponCreationProtocol protocol)
        {
            var weaponEntity = _world.CreateEntity();
            _transformAspectHandler.InitializePositionAndRotationComponents(weaponEntity);
            var weaponConfig = protocol.WeaponConfig;

            var viewOwnerEntity = protocol.ViewOwnerEntity == default ? weaponEntity : protocol.ViewOwnerEntity;

            _parentingRelationsApplier.Apply(new()
            {
                ParentEntity = protocol.ParentEntity,
                ChildEntity = weaponEntity,
                LocalPos = protocol.WeaponAttachmentProtocol.LocalPosition,
                LocalRot = protocol.WeaponAttachmentProtocol.LocalRotation,
                ViewOwnerEntity = viewOwnerEntity
            });

            var constructionProtocol = new WeaponConstructionProtocol()
            {
                WeaponEntity = weaponEntity,
                WeaponConfig = weaponConfig,
                ViewOwnerEntity = viewOwnerEntity,
                AutoStow = protocol.UseAutoStow
            };

            foreach (var featureApplier in _featureAppliers)
            {
                if ((featureApplier.RequiredFlags & weaponConfig.FeatureFlags) == featureApplier.RequiredFlags)
                    featureApplier.Apply(ref constructionProtocol);
            }
           
            ApplyBasicWeaponComponents(weaponEntity, weaponConfig);
            ApplyProtocolSpecificComponents(weaponEntity, protocol);

            return weaponEntity;
        }

        public void ApplyProtocolSpecificComponents(Entity weaponEntity, in WeaponCreationProtocol protocol)
        {
            _weaponComponents.Add(weaponEntity, new(protocol.WeaponControllerEntity));

            if (protocol.AlwaysCalculateShotPoint)
                _alwaysCalculateShotPointsTag.Add(weaponEntity);

            if (protocol.SyncTargetWithParent)
                _syncTargetWithParent.Add(weaponEntity);

            if (protocol.SyncFireTagWithParent)
                _syncFireTagWithParent.Add(weaponEntity);

            if (protocol.DamageParameters.IsValid)
                _damageComponents.Add(weaponEntity, new() { DamageParameters = protocol.DamageParameters });

            if (protocol.UseAutoShot)
                _weaponHandler.OnAutoshotEnabled(weaponEntity);
        }


        // todo: rework to discrete applier if any data needed in other appliers
        private void ApplyBasicWeaponComponents(Entity weaponEntity, IWeaponConfig weaponConfig)
        {
            _ranges.Add(weaponEntity, new(weaponConfig.MinRange, weaponConfig.MaxRange, weaponConfig.RecommendedRangePc));
            _weaponUpdateComponents.Add(weaponEntity, new(weaponConfig.Cooldown));

            if (weaponConfig.TryGetMuzzleEffectId(out var muzzleEffectId))
            {
                var idKey = _stringDictionary.StringToKey(muzzleEffectId);
                var vfxKey = new VfxKey(idKey);
                _muzzleEffects.Add(weaponEntity, new(vfxKey));
            }

            _weaponShotPoints.Add(weaponEntity, new(weaponConfig.ShotPoint));
        }

        
    }
}
