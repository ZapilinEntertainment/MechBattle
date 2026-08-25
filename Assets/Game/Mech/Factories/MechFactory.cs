using Scellecs.Morpeh;
using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;
using System.Collections.Generic;
using System;

namespace ZE.MechBattle
{
    public class MechFactory : IEntityCreationFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly MechConfig TEMP_mechConfig;
        private readonly ProjectileWeaponConfig TEMP_mainWeaponConfig;
        private readonly RayWeaponConfig TEMP_eyesWeaponConfig;

        [Inject]
        public MechFactory(
            IObjectResolver resolver,
            [Key(DevelopConstants.DEFAULT_MECH_ID)] MechConfig mechConfig,
            [Key(DevelopConstants.DEFAULT_MECH_GUN_ID)] ProjectileWeaponConfig weaponConfig,
            [Key(DevelopConstants.LASER_EYES_WEAPON_ID)] RayWeaponConfig eyesWeaponConfig)
        {
            _resolver = resolver;            

            TEMP_mechConfig = mechConfig;
            TEMP_mainWeaponConfig = weaponConfig;
            TEMP_eyesWeaponConfig = eyesWeaponConfig;            
        }

        public Entity Build(float3 position, quaternion rotation)
        {
            var mechConfig = TEMP_mechConfig;

            var mainBuilder = _resolver.Resolve<MechBuilder>();
            var mechEntity = mainBuilder.Build(mechConfig, position, rotation);

            var partsBuilder = _resolver.Resolve<MechPartsBuilder>();
            var chassisFactory = _resolver.Resolve<MechChassisFactory>();
            partsBuilder.AddConstructedPart(MechConstants.CHASSIS_PART_ID, new() { Entity = chassisFactory.Build(mechEntity) });
            partsBuilder.BuildParts(mechEntity, mechConfig);          

            mainBuilder.CheckCrucialParts(partsBuilder);

            var weaponsBuilder = _resolver.Resolve<MechWeaponsBuilder>();
            weaponsBuilder.BuildWeapons(mainBuilder, partsBuilder, mechConfig, TEMP_mainWeaponConfig, TEMP_mainWeaponConfig, TEMP_eyesWeaponConfig);


            return mechEntity;
        }
      
    }
}
