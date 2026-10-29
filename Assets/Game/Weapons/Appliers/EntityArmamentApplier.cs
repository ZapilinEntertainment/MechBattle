using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Weapons
{
    public class EntityArmamentApplier
    {
        private readonly WeaponFactory _weaponFactory;
        private readonly WeaponHandler _weaponHandler;

        [Inject]
        public EntityArmamentApplier(WeaponFactory weaponFactory, WeaponHandler weaponHandler)
        {
            _weaponFactory = weaponFactory;
            _weaponHandler = weaponHandler;
        }

        public void Apply(Entity Entity, EntityArmamentConfig config, WeaponAttachMode weaponAttachMode)
        {
            var isAttachingToBuilding = weaponAttachMode == WeaponAttachMode.Building;
            var weaponEntity = _weaponFactory.CreateWeapon(new()
            {
                ParentEntity = Entity,
                ViewOwnerEntity = Entity,
                WeaponConfig = config.WeaponConfig,
                AttachmentProtocol = config.AttachmentProtocol,

                WeaponControllerEntity = Entity,

                UseAutoStow = !isAttachingToBuilding,
                UseAutoShot = true,                

                DamageParameters = new(config.WeaponConfig.DamageType, config.Damage),
            });

            _weaponHandler.OnAttachedToEntity(weaponEntity, Entity, weaponAttachMode, config.MaxPrecisionAberration);
        }
    }
}
