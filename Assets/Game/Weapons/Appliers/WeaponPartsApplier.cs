using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Weapons
{
    public class WeaponPartsApplier 
    {

        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;
        private readonly World _world;

        private readonly Stash<RotationSpeedComponent> _rotationSpeedComponents;
        private readonly Stash<SyncWithParentTargetTag> _syncTargetWithParent;
        private readonly Stash<ViewPartRequestComponent> _viewPartRequests;

        private readonly Stash<WeaponTowerComponent> _weaponTowerComponents;
        private readonly Stash<WeaponBarrelComponent> _weaponBarrelComponents;
        private readonly Stash<WeaponTowerStowTag> _towerStowTag;
        private readonly Stash<WeaponBarrelStowTag> _barrelStowTag;

        private readonly Stash<LocalRotationLimitComponent> _localRotationLimits;

        [Inject]
        public WeaponPartsApplier(TransformAspectHandler transformAspectHandler, World world, ParentingRelationsApplier parentingRelationsApplier)
        {
            _transformAspectHandler = transformAspectHandler;
            _world = world;
            _parentingRelationsApplier = parentingRelationsApplier;

            _rotationSpeedComponents = _world.GetStash<RotationSpeedComponent>();
            _syncTargetWithParent = _world.GetStash<SyncWithParentTargetTag>();
            _viewPartRequests = _world.GetStash<ViewPartRequestComponent>();

            _weaponTowerComponents = _world.GetStash<WeaponTowerComponent>();
            _weaponBarrelComponents = _world.GetStash<WeaponBarrelComponent>();
            _towerStowTag = world.GetStash<WeaponTowerStowTag>();
            _barrelStowTag = world.GetStash<WeaponBarrelStowTag>();

            _localRotationLimits = world.GetStash<LocalRotationLimitComponent>();
        }

        public Entity AddTower(in WeaponConstructionProtocol constructionProtocol)
        {
            if (!constructionProtocol.WeaponConfig.TryGetFeatureConfig<WeaponTowerFeatureConfig>(out var towerConfig))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError("no tower config defined");
#endif
                return default;
            }
            var weaponEntity = constructionProtocol.WeaponEntity;

            var towerEntity = AttachWeaponPart(constructionProtocol.WeaponEntity, constructionProtocol.ViewOwnerEntity, towerConfig);


            _weaponTowerComponents.Add(weaponEntity, new(towerEntity));
            if (towerConfig.AutoStow & constructionProtocol.AutoStow)
                _towerStowTag.Add(towerEntity);

            if (math.any(towerConfig.FwdRotationLimits.LimitAxleRotation))
                _localRotationLimits.Set(towerEntity, new(towerConfig.FwdRotationLimits));

            return towerEntity;
        }

        public Entity AddBarrel(in WeaponConstructionProtocol constructionProtocol)
        {
            if (!constructionProtocol.WeaponConfig.TryGetFeatureConfig<WeaponBarrelConfig>(out var barrelConfig))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError("no barrel config defined");
#endif
                return default;
            }
            var barrelEntity = AttachWeaponPart(
                parentEntity: constructionProtocol.HaveTower ? constructionProtocol.TowerEntity : constructionProtocol.WeaponEntity, 
                constructionProtocol.ViewOwnerEntity, 
                barrelConfig);

            _weaponBarrelComponents.Add(constructionProtocol.WeaponEntity, new(barrelEntity));

            if (barrelConfig.AutoStow & constructionProtocol.AutoStow)
                _barrelStowTag.Add(barrelEntity);

            if (math.any(barrelConfig.FwdRotationLimits.LimitAxleRotation))
                _localRotationLimits.Set(barrelEntity, new(barrelConfig.FwdRotationLimits));

            return barrelEntity;
        }

        private Entity AttachWeaponPart(Entity parentEntity, Entity viewOwnerEntity, WeaponPartConfigBase config)
        {
            var weaponPartEntity = _world.CreateEntity();
            _transformAspectHandler.InitializePositionAndRotationComponents(weaponPartEntity);

            _parentingRelationsApplier.Apply(new()
            {
                ChildEntity = weaponPartEntity,
                ParentEntity = parentEntity,
                LocalPos = config.LocalPosition,
                LocalRot = quaternion.identity,
                ViewOwnerEntity = viewOwnerEntity
            });

            var rotationSpeedDegrees = config.RotationSpeedDegrees;
            if (rotationSpeedDegrees != 0f)
                _rotationSpeedComponents.Add(weaponPartEntity, new(math.radians(rotationSpeedDegrees)));

            _syncTargetWithParent.Add(weaponPartEntity);
            _viewPartRequests.Add(weaponPartEntity, new(config.ViewPartKey));

            return weaponPartEntity;
        }
    }
}
