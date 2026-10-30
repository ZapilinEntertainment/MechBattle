using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Weapons;

namespace ZE.MechBattle.MechBuilding
{
    public class MechWeaponsBuilder
    {
        private readonly WeaponFactory _weaponFactory;
        private readonly WeaponHandler _weaponHandler;
        private readonly EntityViewHandler _viewHandler;
        private readonly MonoViewFactory _viewFactory;
        private readonly MechWeaponsManager _weaponsManager;
        private readonly AimMethodApplier _aimMethodApplier;

        private readonly Stash<WeaponKeyComponent> _weaponKeyComponents;               
        private readonly Stash<EnergySourceComponent> _energySourceComponents;

        private readonly Stash<WeaponDischargeEnergyConsumption> _dischargeEnergyCosts;
        private readonly Stash<WeaponChargeEnergyConsumption> _chargeEnergyCosts;
        private readonly Stash<ShotEnergyCostComponent> _shotEnergyCosts;
        private readonly Stash<WeaponChargeComponent> _weaponChargeComponents;
        private readonly Stash<ContinuosFiringTag> _continuousFiringTags;

        private MechBuilder _mainBuilder;
        private MechConfig _mechConfig;
        private MechEnergyModuleBuilder _energyModuleBuilder;
        private WeaponConfig _mainWeaponConfigLeft;
        private WeaponConfig _mainWeaponConfigRight;
        private WeaponConfig _laserEyesConfig;

        [Inject]
        public MechWeaponsBuilder(
            World world, 
            WeaponFactory weaponFactory, 
            WeaponHandler weaponHandler,
            EntityViewHandler viewHandler,
            MonoViewFactory viewFactory,
            MechWeaponsManager weaponsManager,
            AimMethodApplier aimMethodApplier
            )
        {
            _weaponFactory = weaponFactory;
            _viewHandler = viewHandler;
            _weaponHandler = weaponHandler;
            _viewFactory = viewFactory;
            _weaponsManager = weaponsManager;
            _aimMethodApplier = aimMethodApplier;

            _weaponKeyComponents = world.GetStash<WeaponKeyComponent>();
            _energySourceComponents = world.GetStash<EnergySourceComponent>();

            _dischargeEnergyCosts = world.GetStash<WeaponDischargeEnergyConsumption>();
            _chargeEnergyCosts = world.GetStash<WeaponChargeEnergyConsumption>();
            _shotEnergyCosts = world.GetStash<ShotEnergyCostComponent>();
            _weaponChargeComponents = world.GetStash<WeaponChargeComponent>();
            _continuousFiringTags = world.GetStash<ContinuosFiringTag>();
        }

        public void BuildWeapons(
            MechBuilder mechBuilder, 
            MechConfig mechConfig,
            MechEnergyModuleBuilder energyModuleBuilder,
            WeaponConfig mainWeaponConfigLeft,
            WeaponConfig mainWeaponConfigRight,
            WeaponConfig laserEyesConfig)
        {
            _mainBuilder = mechBuilder;
            _energyModuleBuilder = energyModuleBuilder;

            _mechConfig = mechConfig;
            _mainWeaponConfigLeft = mainWeaponConfigLeft;
            _mainWeaponConfigRight = mainWeaponConfigRight;
            _laserEyesConfig = laserEyesConfig;

            InstallLaserEyes();
            InstallMainWeapons();
        }

        private void InstallMainWeapons()
        {
            var upperPartEntity = _mainBuilder.UpperPartEntity;
            var mainWeaponLeft = BuildSlotEquipment(upperPartEntity, MechSlot.MainWeaponLeft, DevelopConstants.DEFAULT_MECH_GUN_ID, _mainWeaponConfigLeft);
            var leftWeaponKey = new MechWeaponKey(MechWeaponGroup.Primary, 0);
            _weaponKeyComponents.Set(mainWeaponLeft, new(leftWeaponKey));

            var mainWeaponRight = BuildSlotEquipment(upperPartEntity, MechSlot.MainWeaponRight, DevelopConstants.DEFAULT_MECH_GUN_ID, _mainWeaponConfigRight);
            var rightWeaponKey = new MechWeaponKey(MechWeaponGroup.Primary, 1);
            _weaponKeyComponents.Set(mainWeaponLeft, new(rightWeaponKey));

            var mechEntity = _mainBuilder.MechEntity;
            _weaponsManager.AddWeapon(mechEntity, mainWeaponLeft, leftWeaponKey);
            _weaponsManager.AddWeapon(mechEntity, mainWeaponRight, rightWeaponKey);
        }

        private Entity BuildSlotEquipment(Entity parent, MechSlot slot, string equipmentId, WeaponConfig weaponConfig)
        {
            // todo: different types of equipment, not only weapons
            if (!_mechConfig.TryGetSlotInfo(slot, out var slotInfo))
            {
                UnityEngine.Debug.LogError($"no {slot} slot available");
                return default;
            }

            return BuildMechWeapon(parent, equipmentId, weaponConfig, slotInfo.AttachmentProtocol);
        }

        private Entity BuildMechWeapon(Entity parent, string weaponId, WeaponConfig weaponConfig, ViewPartAttachmentProtocol partAttachmentProtocol)
        {
            var weaponEntity = _weaponFactory.CreateWeapon(new()
            {
                WeaponConfig = weaponConfig,
                ParentEntity = parent,
                WeaponControllerEntity = _mainBuilder.MechEntity,

                WeaponAttachmentProtocol = partAttachmentProtocol,
                SyncTargetWithParent = true,
                AlwaysCalculateShotPoint = true,

                DamageParameters = new(weaponConfig.DamageType, DevelopConstants.TEMP_MainGunDamage),
            });
            _viewFactory.MakeViewReceiver(weaponEntity, weaponId + "_view");

            AddMechSpecificComponents(weaponEntity);

            return weaponEntity;
        }

        private void InstallLaserEyes()
        {
            // head entity works as weapon entity for all eyes

            var settings = _mechConfig.MechPartSettings;
            var headEntity = _mainBuilder.HeadEntity;
            var mechEntity = _mainBuilder.MechEntity;
            Entity weaponEntity = default;

            foreach (var partSettings in settings)
            {
                if (partSettings.SpecialKeywords.Contains(MechConstants.EYE_KEYWORD))
                {
                    weaponEntity = BuildLaserEye(headEntity, partSettings);

                    // todo: how to build parent-controller weapons with crucial component transitions? check transition move below
                    // remove weapons charge component and set it only on parent
                    TransferComponentsCommand.Execute(weaponEntity, headEntity, _weaponChargeComponents, _chargeEnergyCosts, _dischargeEnergyCosts);
                }                    
            }

            // transite weapon components to their parent (head entity) - hardcoded, need weapon-related transition
            // note: to copy from other entity should do World.Commit first (cannot be done inside filter block)
            var weaponKey = new MechWeaponKey(MechWeaponGroup.Eyes, 0);            
            _weaponKeyComponents.Set(headEntity, new(weaponKey));
            SyncComponentsCommand.Execute<ContinuosFiringTag>(headEntity, weaponEntity, _continuousFiringTags);
            SyncComponentsCommand.Execute<ShotEnergyCostComponent>(headEntity, weaponEntity, _shotEnergyCosts);
                      
            // write to weapons manager as single weapon
            _weaponsManager.AddWeapon(mechEntity, headEntity, weaponKey);

            _energySourceComponents.Set(headEntity, new(_energyModuleBuilder.ReactorEntity));
        }

        private Entity BuildLaserEye(Entity headEntity, MechPartSettings constructionSettings)
        {
            var eyeEntity = _weaponFactory.CreateWeapon(new()
            {
                WeaponAttachmentProtocol = constructionSettings.AttachProtocol,
                DamageParameters = new(_laserEyesConfig.DamageType, DevelopConstants.TEMP_EyesDamage),
                WeaponConfig = _laserEyesConfig,
                ParentEntity = headEntity,
                SyncTargetWithParent = true,
                SyncFireTagWithParent = true,
                AlwaysCalculateShotPoint = true,
                ViewOwnerEntity = _mainBuilder.MechEntity,
                WeaponControllerEntity = _mainBuilder.MechEntity,              
            });

            var barrel = _weaponHandler.GetBarrelEntity(eyeEntity);
            _viewHandler.OverrideViewRequestKey(barrel, constructionSettings.Key);

          //  eyeEntity.AddComponent<WeaponDirectAimTag>();

            AddMechSpecificComponents(eyeEntity);

            return eyeEntity;
        }

        private void AddMechSpecificComponents(Entity weaponEntity)
        {
            _energySourceComponents.Set(weaponEntity, new(_energyModuleBuilder.ReactorEntity));
            _aimMethodApplier.AddMethodTag(weaponEntity);
        }
    }
}
