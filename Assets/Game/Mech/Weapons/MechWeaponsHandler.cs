using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle;

namespace ZE.MechBattle
{
    public class MechWeaponsHandler
    {
        private readonly EnergyHandler _energyHandler;
        private readonly PartitionsListManager _parititionsManager;
        private readonly Stash<EnergyChargeComponent> _energyCharges;
        private readonly Stash<EnergyConsumerComponent> _energyConsumption;
        private readonly Stash<WeaponLoadingComponent> _loadingComponents;
        private readonly Stash<GunLoadedTag> _gunLoadedTags;

        public MechWeaponsHandler(World world, EnergyHandler energyHandler, PartitionsListManager partitionsListManager)
        {
            _energyHandler = energyHandler;
            _parititionsManager = partitionsListManager;

            _energyCharges = world.GetStash<EnergyChargeComponent>();
            _energyConsumption = world.GetStash<EnergyConsumerComponent>();
            _loadingComponents = world.GetStash<WeaponLoadingComponent>();
            _gunLoadedTags = world.GetStash<GunLoadedTag>();
        }

        public bool CanWeaponFire(Entity mechEntity, Entity weaponEntity)
        {
            return _gunLoadedTags.Has(weaponEntity) && GetWeaponEnergySatisfactionPc(mechEntity, weaponEntity) >= 1f;
        }

        public void FireWeapon(Entity mechEntity, Entity weaponEntity)
        {
            var shotConsumption = _energyConsumption.Get(weaponEntity).Volume;
            var partitions = _parititionsManager.GetPartitionsList(mechEntity);
            _energyHandler.SpendEnergyFromPartitions(mechEntity, partitions, shotConsumption);
        }

        public float GetWeaponEnergySatisfactionPc(Entity mechEntity, Entity weaponEntity)
        {
            // note: only simple projectile gun logic atm
            var mechCharge = _energyCharges.Get(mechEntity).Value;
            var consumptionComponent = _energyConsumption.Get(weaponEntity, out var requiresEnergy);
            if (!requiresEnergy)
                return 1f;

            var shotConsumption = consumptionComponent.Volume;
            if (mechCharge >= shotConsumption)
                return 1f;
            else
                return mechCharge / shotConsumption;
        }

        public float GetWeaponLoadingProgress(Entity weaponEntity)
        {
            if (_gunLoadedTags.Has(weaponEntity))
                return 1f;

            var loadingComponent = _loadingComponents.Get(weaponEntity, out var loadingExists);
            return loadingExists ? loadingComponent.LoadingPc : 1f;
        }
    }
}
