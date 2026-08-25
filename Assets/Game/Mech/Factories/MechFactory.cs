using Scellecs.Morpeh;
using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;
using System.Collections.Generic;

namespace ZE.MechBattle
{
    public class MechFactory : IEntityCreationFactory
    {
        private readonly MechChassisFactory _chassisFactory;
        private readonly MonoViewFactory _viewFactory;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;
        private readonly WeaponHandler _weaponHandler;
        private readonly EntityViewHandler _viewHandler;
        private readonly WeaponFactory _weaponFactory;
        private readonly MechPartitionFactory _partitionsFactory;
        private readonly World _world;

        private readonly MechConfig TEMP_mechConfig;
        private readonly ProjectileWeaponConfig TEMP_mainWeaponConfig;
        private readonly RayWeaponConfig TEMP_eyesWeaponConfig;
        

        private readonly Stash<MechComponent> _mechComponents;
        private readonly Stash<MechWeaponsComponent> _mechWeapons;

        private const float TEMP_MainGunDamage = 10f;
        private const float TEMP_EyesDamage = 100f;

        [Inject]
        public MechFactory(
            MonoViewFactory viewFactory, 
            TransformAspectHandler transformAspectHandler, 
            EntityViewHandler viewHandler,
            WeaponHandler weaponHandler,

            MechChassisFactory chassisFactory,
            World world,
            ParentingRelationsApplier parentingRelationsApplier,
            WeaponFactory weaponFactory,
            MechPartitionFactory partitionFactory,

            [Key(DevelopConstants.DEFAULT_MECH_ID)] MechConfig mechConfig,
            [Key(DevelopConstants.DEFAULT_MECH_GUN_ID)] ProjectileWeaponConfig weaponConfig,
            [Key(DevelopConstants.LASER_EYES_WEAPON_ID)] RayWeaponConfig eyesWeaponConfig)
        {
            _viewFactory = viewFactory;

            _transformAspectHandler = transformAspectHandler;
            _weaponHandler = weaponHandler;
            _viewHandler = viewHandler;

            _chassisFactory = chassisFactory;
            _parentingRelationsApplier = parentingRelationsApplier;            
            _weaponFactory = weaponFactory;
            _partitionsFactory = partitionFactory;

            TEMP_mechConfig = mechConfig;
            TEMP_mainWeaponConfig = weaponConfig;
            TEMP_eyesWeaponConfig = eyesWeaponConfig;

            _world = world;
            _mechComponents = _world.GetStash<MechComponent>();
            _mechWeapons = _world.GetStash<MechWeaponsComponent>();
        }

        public Entity Build(float3 position, quaternion rotation)
        {
            var mechConfig = TEMP_mechConfig;
            var builder = new MechBuilder(mechConfig, _parentingRelationsApplier, _world);

            var mechEntity = _viewFactory.CreateViewReceiver(DevelopConstants.DEFAULT_MECH_ID + "_view");
            _transformAspectHandler.MoveToPoint(mechEntity, position, rotation);
            builder.MechEntity = mechEntity;            

            var chassisEntity = _chassisFactory.Build(mechEntity);
            builder.AddConstructedPart(MechConstants.CHASSIS_PART_ID, new() { Entity = chassisEntity });

            var eyeSettingsList = new List<MechPartSettings>();
            foreach (var mechPartKvp in mechConfig.MechPartSettings)
            {
                var mechPartSettings = mechPartKvp.Value;
                var constructionMode = mechPartSettings.ConstructProtocol.ConstructionMode;
                if (constructionMode == ViewPartConstructionMode.SpecialMode)
                {
                    if (mechPartSettings.SpecialKeywords.Contains(MechConstants.EYE_KEYWORD))
                    {
                        eyeSettingsList.Add(mechPartSettings);
                    }
                    else
                    {
                        throw new System.NotImplementedException("undefined part special construction logic: " + mechPartKvp.Key);
                    }
                }
                else
                {
                    builder.TryBuildPart(mechPartKvp.Key);
                }                
            }


            if (builder.TryGetConstructedPartEntity(MechConstants.HEAD_PART_ID, out var headEntity))
            {
                foreach (var eyeConstructionSettings in eyeSettingsList)
                {
                    var eyeEntity = BuildLaserEye(headEntity, eyeConstructionSettings);
                }
            }               

            if (builder.TryGetConstructedPartEntity(MechConstants.UPPER_PART_ID, out var upperPartEntity))
            {
                var mainWeaponLeft = InstallEquipmentIntoSlot(upperPartEntity, mechConfig, MechSlot.MainWeaponLeft, DevelopConstants.DEFAULT_MECH_GUN_ID);
                var mainWeaponRight = InstallEquipmentIntoSlot(upperPartEntity, mechConfig, MechSlot.MainWeaponRight, DevelopConstants.DEFAULT_MECH_GUN_ID);
                _mechWeapons.Add(mechEntity, new()
                {
                    MainWeaponLeft = mainWeaponLeft,
                    MainWeaponRight = mainWeaponRight,
                });
            }
            else
            {
                UnityEngine.Debug.LogError("upper part not found");
            }
            

            _mechComponents.Add(mechEntity, new(chassisEntity, upperPartEntity, headEntity));
           // _partitionsFactory.CreatePartitions(mechEntity, mechConfig);

            return mechEntity;
        }

        private Entity InstallEquipmentIntoSlot(Entity parent, MechConfig mechConfig, MechSlot slot, string equipmentId)
        {
            // todo: different types of equipment, not only weapons
            if (!mechConfig.TryGetSlotInfo(slot, out var slotInfo))
            {
                UnityEngine.Debug.LogError($"no {slot} slot available");
                return default;
            }

            var weaponEntity = _weaponFactory.CreateWeapon(new()
            {
                WeaponConfig = TEMP_mainWeaponConfig,
                ParentEntity = parent,
                AttachmentProtocol = slotInfo.AttachmentProtocol,
                SyncTargetWithParent = true,

                DamageParameters = new(TEMP_mainWeaponConfig.DamageType, TEMP_MainGunDamage)
            });
            _viewFactory.MakeViewReceiver(weaponEntity, equipmentId + "_view");

            return weaponEntity;
        }

        private Entity BuildLaserEye(Entity headEntity, MechPartSettings constructionSettings)
        {
            var eyeEntity = _weaponFactory.CreateWeapon(new()
            {
                AttachmentProtocol = constructionSettings.AttachProtocol,
                DamageParameters = new(TEMP_eyesWeaponConfig.DamageType, TEMP_EyesDamage),
                WeaponConfig = TEMP_eyesWeaponConfig,
                ParentEntity = headEntity,
                SyncTargetWithParent = true,
                SyncFireTagWithParent = true
            });

            var barrel = _weaponHandler.GetBarrelEntity(eyeEntity);
            _viewHandler.OverrideViewRequestKey(barrel, constructionSettings.ConstructProtocol.ViewPartKey);

            return eyeEntity;
        }
    }
}
