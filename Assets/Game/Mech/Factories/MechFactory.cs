using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.MechBuilding;

namespace ZE.MechBattle
{
    public class MechFactory : IEntityCreationFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly ColliderAddRequestsFactory _collidersRequestsFactory;

        private readonly MechConfig TEMP_mechConfig;
        private readonly ProjectileWeaponConfig TEMP_mainWeaponConfig;
        private readonly RayWeaponConfig TEMP_eyesWeaponConfig;

        [Inject]
        public MechFactory(
            IObjectResolver resolver,
            ColliderAddRequestsFactory collidersRequestsFactory,

            [Key(DevelopConstants.DEFAULT_MECH_ID)] MechConfig mechConfig,
            [Key(DevelopConstants.DEFAULT_MECH_GUN_ID)] ProjectileWeaponConfig weaponConfig,
            [Key(DevelopConstants.LASER_EYES_WEAPON_ID)] RayWeaponConfig eyesWeaponConfig)
        {
            _resolver = resolver;
            _collidersRequestsFactory = collidersRequestsFactory;

            TEMP_mechConfig = mechConfig;
            TEMP_mainWeaponConfig = weaponConfig;
            TEMP_eyesWeaponConfig = eyesWeaponConfig;            
        }

        public Entity Build(float3 position, quaternion rotation)
        {
            var mechConfig = TEMP_mechConfig;

            var mainBuilder = _resolver.Resolve<MechBuilder>();
            var mechEntity = mainBuilder.Build(mechConfig, position, rotation);
            var separatedPartKeys = PrepareSeparatingKeysList(mechConfig);

            var bitsBuilder = _resolver.Resolve<MechBitsBuilder>();
            var chassisFactory = _resolver.Resolve<MechChassisFactory>();
            RegisterAllChassisParts(chassisFactory.Build(mechEntity, separatedPartKeys), bitsBuilder);            
            bitsBuilder.BuildParts(mechEntity, mechConfig, separatedPartKeys);          

            mainBuilder.CheckCrucialParts(bitsBuilder);

            var weaponsBuilder = _resolver.Resolve<MechWeaponsBuilder>();
            weaponsBuilder.BuildWeapons(mainBuilder, mechConfig, TEMP_mainWeaponConfig, TEMP_mainWeaponConfig, TEMP_eyesWeaponConfig);

            var partitionsBuilder = _resolver.Resolve<MechPartitionBuilder>();
            partitionsBuilder.BuildAllPartitions(mechEntity, bitsBuilder, mechConfig.PartitionConfigs);

            RequestColliders(mechEntity, bitsBuilder, mechConfig, partitionsBuilder.PartitionsList);

            //foreach (var part in bitsBuilder.ConstructedParts) UnityEngine.Debug.Log($"{part.Key} : {part.Value.Id}");

            return mechEntity;
        }

        private HashSet<ViewPartKey> PrepareSeparatingKeysList(MechConfig mechConfig)
        {
            var set = new HashSet<ViewPartKey>();
            foreach (var collider in mechConfig.ColliderConfigs)
            {
                set.Add(collider.Key);
            }
            return set;
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
        private void RequestColliders(Entity mechEntity, MechBitsBuilder mechPartsBuilder, MechConfig mechConfig, IPartitionsList partitionsList)
        {
            foreach (var colliderConfig in mechConfig.ColliderConfigs)
            {
                // check collider host
                if (!mechPartsBuilder.TryGetConstructedPartEntity(colliderConfig.Key, out var constructedPartEntity))
                {
                    UnityEngine.Debug.LogWarning($"part {colliderConfig.Key} was not constructed");
                    continue;
                }

                // check partition
                var partition = colliderConfig.PartitionKey;
                Entity colliderOwner;
                if (partition.Type == MechPartitionType.Undefined)
                {
                    colliderOwner = mechEntity;
                }
                else
                {
                    if (!partitionsList.TryGet(partition, out var partitionEntity))
                    {
                        UnityEngine.Debug.LogWarning($"{colliderConfig.PartitionKey} partition set to {partition}, which was not constructed");
                        continue;
                    }
                    colliderOwner = partitionEntity;
                }

                _collidersRequestsFactory.CreateRequest(
                    new ColliderAddRequestComponent(
                        ownerEntity: colliderOwner, 
                        hostEntity: constructedPartEntity, 
                        setupInfo: colliderConfig.ColliderSetupInfo));
            }            
        }
    }
}
