using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.MechBuilding;

namespace ZE.MechBattle
{
    public class MechFactory : IEntityCreationFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly CollidersFactory _collidersFactory;

        private readonly MechConfig TEMP_mechConfig;
        private readonly ProjectileWeaponConfig TEMP_mainWeaponConfig;
        private readonly RayWeaponConfig TEMP_eyesWeaponConfig;

        [Inject]
        public MechFactory(
            IObjectResolver resolver,
            CollidersFactory collidersFactory,

            [Key(DevelopConstants.DEFAULT_MECH_ID)] MechConfig mechConfig,
            [Key(DevelopConstants.DEFAULT_MECH_GUN_ID)] ProjectileWeaponConfig weaponConfig,
            [Key(DevelopConstants.LASER_EYES_WEAPON_ID)] RayWeaponConfig eyesWeaponConfig)
        {
            _resolver = resolver;
            _collidersFactory = collidersFactory;

            TEMP_mechConfig = mechConfig;
            TEMP_mainWeaponConfig = weaponConfig;
            TEMP_eyesWeaponConfig = eyesWeaponConfig;            
        }

        public Entity Build(float3 position, quaternion rotation)
        {
            var mechConfig = TEMP_mechConfig;

            var mainBuilder = _resolver.Resolve<MechBuilder>();
            var mechEntity = mainBuilder.Build(mechConfig, position, rotation);

            var bitsBuilder = _resolver.Resolve<MechBitsBuilder>();
            var chassisFactory = _resolver.Resolve<MechChassisFactory>();
            RegisterAllChassisParts(chassisFactory.Build(mechEntity), bitsBuilder);

            bitsBuilder.BuildParts(mechEntity, mechConfig);          

            mainBuilder.CheckCrucialParts(bitsBuilder);

            var weaponsBuilder = _resolver.Resolve<MechWeaponsBuilder>();
            weaponsBuilder.BuildWeapons(mainBuilder, bitsBuilder, mechConfig, TEMP_mainWeaponConfig, TEMP_mainWeaponConfig, TEMP_eyesWeaponConfig);

            var partitionsBuilder = _resolver.Resolve<MechPartitionBuilder>();
            partitionsBuilder.BuildAllPartitions(mechEntity, bitsBuilder, mechConfig.PartitionConfigs);

            BuildColliders(mechEntity, bitsBuilder, mechConfig, partitionsBuilder.PartitionsList);

            return mechEntity;
        }

        private void RegisterAllChassisParts(MechChassisFactory.ChassisEntities chassisEntities, MechBitsBuilder bitsBuilder)
        {
            bitsBuilder.AddConstructedPart(ViewPartKey.Chassis, chassisEntities.ChassisRoot);

            bitsBuilder.AddConstructedPart(ViewPartKey.GetHipKey(false), chassisEntities.LeftLeg.Hip );
            bitsBuilder.AddConstructedPart(ViewPartKey.GetAnkleKey(false), chassisEntities.LeftLeg.Ankle);
            bitsBuilder.AddConstructedPart(ViewPartKey.GetFootKey(false), chassisEntities.LeftLeg.Foot);

            bitsBuilder.AddConstructedPart(ViewPartKey.GetHipKey(true), chassisEntities.RightLeg.Hip);
            bitsBuilder.AddConstructedPart(ViewPartKey.GetAnkleKey(true), chassisEntities.RightLeg.Ankle);
            bitsBuilder.AddConstructedPart(ViewPartKey.GetFootKey(true), chassisEntities.RightLeg.Foot);
        }
      

        // build colliders GO on part view containers (through part entity),
        // but set their ownity to partition entity or mech (if partition not defined)
        private void BuildColliders(Entity mechEntity, MechBitsBuilder mechPartsBuilder, MechConfig mechConfig, IPartitionsList partitionsList)
        {
            //foreach (var colliderConfig in mechConfig.ColliderConfigs)
            //{

            //}

            //foreach (var partSettingKvp in mechConfig.MechPartSettings)
            //{
            //    var collidersConfig = partSettingKvp.Value.CollidersConfig;
            //    if (collidersConfig == null || collidersConfig.Length == 0)
            //        continue;

            //    if (!mechPartsBuilder.TryGetConstructedPartEntity(partSettingKvp.Key, out var constructedPartEntity))
            //    {
            //        UnityEngine.Debug.LogWarning($"part {partSettingKvp.Key} was not constructed");
            //        continue;
            //    }

            //    var partition = partSettingKvp.Value.Partition;
            //    Entity colliderOwner;
            //    if (partition.Type == MechPartitionType.Undefined)
            //    {
            //        colliderOwner = mechEntity;
            //    }
            //    else
            //    {
            //        if (!partitionsList.TryGet(partition, out var partitionEntity))
            //        {
            //            UnityEngine.Debug.LogWarning($"{partSettingKvp.Key} partition set to {partition}, which was not constructed");
            //            continue;
            //        }
            //        colliderOwner = partitionEntity;
            //    }
                

            //    foreach (var colliderConfig in collidersConfig)
            //    {
            //        _collidersFactory.BuildCollider(colliderOwner, constructedPartEntity, colliderConfig);
            //    }
            //}
        }
    }
}
