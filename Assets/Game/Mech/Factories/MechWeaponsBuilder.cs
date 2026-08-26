using Scellecs.Morpeh;
using System.Collections.Generic;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.MechBuilding
{
    public class MechWeaponsBuilder
    {
        private readonly WeaponFactory _weaponFactory;
        private readonly WeaponHandler _weaponHandler;
        private readonly EntityViewHandler _viewHandler;
        private readonly MonoViewFactory _viewFactory;
        private readonly Stash<MechWeaponsComponent> _mechWeapons;

        private MechBuilder _mainBuilder;
        private MechConfig _mechConfig;
        private WeaponConfigBase _mainWeaponConfigLeft;
        private WeaponConfigBase _mainWeaponConfigRight;
        private WeaponConfigBase _laserEyesConfig;

        [Inject]
        public MechWeaponsBuilder(
            World world, 
            WeaponFactory weaponFactory, 
            WeaponHandler weaponHandler,
            EntityViewHandler viewHandler,
            MonoViewFactory viewFactory
            )
        {
            _weaponFactory = weaponFactory;
            _viewHandler = viewHandler;
            _weaponHandler = weaponHandler;
            _viewFactory = viewFactory;

            _mechWeapons = world.GetStash<MechWeaponsComponent>();
        }

        public void BuildWeapons(
            MechBuilder mechBuilder, 
            MechConfig mechConfig, 
            WeaponConfigBase mainWeaponConfigLeft,
            WeaponConfigBase mainWeaponConfigRight,
            WeaponConfigBase laserEyesConfig)
        {
            _mainBuilder = mechBuilder;

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
            var mainWeaponRight = BuildSlotEquipment(upperPartEntity, MechSlot.MainWeaponRight, DevelopConstants.DEFAULT_MECH_GUN_ID, _mainWeaponConfigRight);
            _mechWeapons.Add(_mainBuilder.MechEntity, new()
            {
                MainWeaponLeft = mainWeaponLeft,
                MainWeaponRight = mainWeaponRight,
            });
        }

        private Entity BuildSlotEquipment(Entity parent, MechSlot slot, string equipmentId, WeaponConfigBase weaponConfig)
        {
            // todo: different types of equipment, not only weapons
            if (!_mechConfig.TryGetSlotInfo(slot, out var slotInfo))
            {
                UnityEngine.Debug.LogError($"no {slot} slot available");
                return default;
            }

            var weaponEntity = _weaponFactory.CreateWeapon(new()
            {
                WeaponConfig = weaponConfig,
                ParentEntity = parent,
                AttachmentProtocol = slotInfo.AttachmentProtocol,
                SyncTargetWithParent = true,

                DamageParameters = new(weaponConfig.DamageType, DevelopConstants.TEMP_MainGunDamage),
            });
            _viewFactory.MakeViewReceiver(weaponEntity, equipmentId + "_view");

            return weaponEntity;
        }

        private void InstallLaserEyes()
        {
            var settings = _mechConfig.MechPartSettings;
            var headEntity = _mainBuilder.HeadEntity;
            foreach (var partSettings in settings)
            {
                if (partSettings.SpecialKeywords.Contains(MechConstants.EYE_KEYWORD))
                    BuildLaserEye(headEntity, partSettings);
            }
        }

        private Entity BuildLaserEye(Entity headEntity, MechPartSettings constructionSettings)
        {
            var eyeEntity = _weaponFactory.CreateWeapon(new()
            {
                AttachmentProtocol = constructionSettings.AttachProtocol,
                DamageParameters = new(_laserEyesConfig.DamageType, DevelopConstants.TEMP_EyesDamage),
                WeaponConfig = _laserEyesConfig,
                ParentEntity = headEntity,
                SyncTargetWithParent = true,
                SyncFireTagWithParent = true,
                ViewOwnerEntity = _mainBuilder.MechEntity
                
            });

            var barrel = _weaponHandler.GetBarrelEntity(eyeEntity);
            _viewHandler.OverrideViewRequestKey(barrel, constructionSettings.Key);

            return eyeEntity;
        }

    }
}
