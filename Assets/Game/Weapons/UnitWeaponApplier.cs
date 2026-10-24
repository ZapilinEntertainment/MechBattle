using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class UnitWeaponApplier : UnitFeatureApplierBase<UnitWeaponConfig>
    {
        private readonly WeaponFactory _weaponFactory;
        private readonly WeaponHandler _weaponHandler;

        [Inject]
        public UnitWeaponApplier(WeaponFactory weaponFactory, WeaponHandler weaponHandler)
        {
            _weaponFactory = weaponFactory;
            _weaponHandler = weaponHandler;
        }

        protected override void ApplyUnitFeature(Entity unitEntity, UnitWeaponConfig weaponData)
        {
            var weaponEntity = _weaponFactory.CreateWeapon(new()
            {
                ParentEntity = unitEntity,
                ViewOwnerEntity = unitEntity,
                WeaponConfig = weaponData.ProjectileConfig,
                AttachmentProtocol = weaponData.AttachmentProtocol,

                WeaponControllerEntity = unitEntity,

                UseAutoStow = true,
                UseAutoShot = true,

                DamageParameters = new(weaponData.ProjectileConfig.DamageType, weaponData.Damage),
            });

            _weaponHandler.OnAttachedToUnit(weaponEntity, unitEntity, weaponData.MaxPrecisionAberration);
        }
    }
}
