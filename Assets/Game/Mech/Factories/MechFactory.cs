using Scellecs.Morpeh;
using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechFactory : IEntityCreationFactory
    {
        private readonly MechChassisFactory _chassisFactory;
        private readonly MonoViewFactory _viewFactory;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;        
        private readonly WeaponFactory _weaponFactory;

        private readonly MechConfig TEMP_mechConfig;
        private readonly WeaponConfig TEMP_weaponConfig;

        private readonly Stash<MechComponent> _mechComponents;
        private readonly Stash<RotationSpeedComponent> _rotationSpeed;
        private readonly Stash<MechWeaponsComponent> _mechWeapons;
        private readonly Stash<LocalRotationLimitComponent> _localRotationLimits;


        [Inject]
        public MechFactory(
            MonoViewFactory viewFactory, 
            TransformAspectHandler transformAspectHandler, 
            MechChassisFactory chassisFactory,
            World world,
            ParentingRelationsApplier parentingRelationsApplier,
            WeaponFactory weaponFactory,
            [Key(DevelopConstants.DEFAULT_MECH_ID)] MechConfig mechConfig,
            [Key(DevelopConstants.DEFAULT_MECH_GUN_ID)] WeaponConfig weaponConfig)
        {
            _viewFactory = viewFactory;
            _transformAspectHandler = transformAspectHandler;
            _chassisFactory = chassisFactory;
            _parentingRelationsApplier = parentingRelationsApplier;            
            _weaponFactory = weaponFactory;

            TEMP_mechConfig = mechConfig;
            TEMP_weaponConfig = weaponConfig;

            _mechComponents = world.GetStash<MechComponent>();
            _rotationSpeed = world.GetStash<RotationSpeedComponent>();
            _mechWeapons = world.GetStash<MechWeaponsComponent>();
            _localRotationLimits = world.GetStash<LocalRotationLimitComponent>();
        }

        public Entity Build(float3 position, quaternion rotation)
        {
            var mechEntity = _viewFactory.CreateViewReceiver(DevelopConstants.DEFAULT_MECH_ID + "_view");
            _transformAspectHandler.MoveToPoint(mechEntity, position, rotation);

            var chassisEntity = _chassisFactory.Build(mechEntity);
            var upperPartEntity = BuildUpperPart(chassisEntity, mechEntity);
            var headEntity = BuildHead(upperPartEntity, TEMP_mechConfig);

            _mechComponents.Add(mechEntity, new(chassisEntity, upperPartEntity, headEntity));            

            return mechEntity;
        }

        private Entity BuildUpperPart(Entity parent, Entity mechEntity)
        {
            var upperPartEntity = _parentingRelationsApplier.CreateChildEntityForViewPart(
               new(quaternion.identity, float3.zero),
               parent,
               new(ViewPartType.UpperPart));

            _rotationSpeed.Set(upperPartEntity, new(TEMP_mechConfig.UpperPartRotationSpeedRadians));

            var mainWeaponLeft = InstallEquipmentIntoSlot(upperPartEntity, TEMP_mechConfig, MechSlot.MainWeaponLeft, DevelopConstants.DEFAULT_MECH_GUN_ID);
            var mainWeaponRight = InstallEquipmentIntoSlot(upperPartEntity, TEMP_mechConfig, MechSlot.MainWeaponRight, DevelopConstants.DEFAULT_MECH_GUN_ID);
            _mechWeapons.Add(mechEntity, new() { 
                MainWeaponLeft = mainWeaponLeft, 
                MainWeaponRight = mainWeaponRight });

            return upperPartEntity;
        }

        private Entity InstallEquipmentIntoSlot(Entity parent, MechConfig mechConfig, MechSlot slot, string equipmentId)
        {
            // todo: different types of equipment, not only weapons
            if (!mechConfig.TryGetSlotInfo(slot, out var slotInfo))
            {
                UnityEngine.Debug.LogError($"no {slot} slot available");
                return default;
            }

            const float TEMP_Damage = 10f;

            var weaponEntity = _weaponFactory.CreateWeapon(new()
            {
                WeaponConfig = TEMP_weaponConfig,
                ParentEntity = parent,
                AttachmentProtocol = slotInfo.AttachmentProtocol,
                SyncTargetWithParent = true,

                DamageParameters = new(TEMP_Damage)
            });
            _viewFactory.MakeViewReceiver(weaponEntity, equipmentId + "_view");

            return weaponEntity;
        }

        private Entity BuildHead(Entity parent, MechConfig mechConfig)
        {
            var attachmentProtocol = mechConfig.HeadAttachmentProtocol;
            var headEntity = _parentingRelationsApplier.CreateChildEntityForViewPart(
               attachmentProtocol.ToPoint(),
               parent,
               new(ViewPartType.Head));

            _rotationSpeed.Set(headEntity, new(mechConfig.HeadRotationSpeedRadians));
            _localRotationLimits.Set(headEntity, new(mechConfig.HeadRotationLimits));

            return headEntity;
        }
    }
}
