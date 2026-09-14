using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechWeaponsHandler
    {
        private readonly EnergyHandler _energyHandler;
        private readonly PartitionsListManager _parititionsManager;
        private readonly Stash<EnergyChargeComponent> _energyCharges;
        private readonly Stash<ShotEnergyCostComponent> _energyCosts;
        private readonly Stash<WeaponLoadingComponent> _loadingComponents;
        private readonly Stash<GunLoadedTag> _gunLoadedTags;

        public MechWeaponsHandler(World world, EnergyHandler energyHandler, PartitionsListManager partitionsListManager)
        {
            _energyHandler = energyHandler;
            _parititionsManager = partitionsListManager;

            _energyCharges = world.GetStash<EnergyChargeComponent>();
            _energyCosts = world.GetStash<ShotEnergyCostComponent>();
            _loadingComponents = world.GetStash<WeaponLoadingComponent>();
            _gunLoadedTags = world.GetStash<GunLoadedTag>();
        }

        public bool CanWeaponFire(Entity mechEntity, Entity weaponEntity)
        {
            return _gunLoadedTags.Has(weaponEntity) && GetWeaponEnergySatisfactionPc(mechEntity, weaponEntity) >= 1f;
        }

        public bool TrySpendEnergyForWeaponShot(Entity mechEntity, Entity weaponEntity, out float shortage)
        {
            var shotCost = _energyCosts.Get(weaponEntity).Volume;
            var partitions = _parititionsManager.GetPartitionsList(mechEntity);
            return _energyHandler.TrySpendEnergyFromPartitions(mechEntity, partitions, shotCost, out shortage);
        }

        public float GetWeaponEnergySatisfactionPc(Entity mechEntity, Entity weaponEntity)
        {
            // note: only simple projectile gun logic atm
            var mechCharge = _energyCharges.Get(mechEntity).Value;
            var costComponent = _energyCosts.Get(weaponEntity, out var requiresEnergy);
            if (!requiresEnergy)
                return 1f;

            var shotConsumption = costComponent.Volume;
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
