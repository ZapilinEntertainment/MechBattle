using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class UnitWeaponApplier : IUnitFeatureApplier
    {
        private readonly WeaponFactory _weaponFactory;
        private readonly WeaponHandler _weaponHandler;

        [Inject]
        public UnitWeaponApplier(WeaponFactory weaponFactory, WeaponHandler weaponHandler)
        {
            _weaponFactory = weaponFactory;
            _weaponHandler = weaponHandler;
        }

        public void ApplyUnitFeature(Entity unitEntity, IUnitConfig config)
        {
            if (!config.TryGetWeaponData(out var weaponData))
                return;

            var weaponEntity = _weaponFactory.CreateWeapon(new()
            {
                ParentEntity = unitEntity,
                ViewOwnerEntity = unitEntity,
                WeaponConfig = weaponData.Config,
                AttachmentProtocol = weaponData.AttachmentProtocol,

                WeaponControllerEntity = unitEntity,

                UseAutoStow = true,
                UseAutoShot = true,

                DamageParameters = new(weaponData.Config.DamageType, config.Damage),
            });

            _weaponHandler.OnAttachedToUnit(weaponEntity, unitEntity, config.MaxPrecisionAberration);
        }
    }
}
