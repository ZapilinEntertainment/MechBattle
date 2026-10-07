using Scellecs.Morpeh;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Weapons;

namespace ZE.MechBattle
{
    public class MechWeaponsHandler
    {
        private readonly EnergyHandler _energyHandler;
        private readonly PartitionsListManager _parititionsManager;
        private readonly MechWeaponsManager _weaponsManager;
        private readonly WeaponHandler _weaponHandler;

        private readonly Stash<EnergyChargeComponent> _energyCharges;
        private readonly Stash<ShotEnergyCostComponent> _energyCosts;
        private readonly Stash<WeaponLoadingComponent> _loadingComponents;
        private readonly Stash<GunLoadedTag> _gunLoadedTags;
        private readonly Stash<WeaponRangeComponent> _weaponRanges;

        public MechWeaponsHandler(
            World world, 
            EnergyHandler energyHandler, 
            PartitionsListManager partitionsListManager, 
            MechWeaponsManager weaponsManager,
            WeaponHandler weaponHandler)
        {
            _energyHandler = energyHandler;
            _parititionsManager = partitionsListManager;
            _weaponsManager = weaponsManager;
            _weaponHandler = weaponHandler;

            _energyCharges = world.GetStash<EnergyChargeComponent>();
            _energyCosts = world.GetStash<ShotEnergyCostComponent>();
            _loadingComponents = world.GetStash<WeaponLoadingComponent>();
            _gunLoadedTags = world.GetStash<GunLoadedTag>();
            _weaponRanges = world.GetStash<WeaponRangeComponent>();
        }

        public void SetupWeaponsForBotControl(Entity mechEntity, float maxPrecisionAberration)
        {
            foreach (var weaponEntity in _weaponsManager.GetNextGroupWeapon(mechEntity, MechWeaponGroup.Primary))
            {
                _weaponHandler.OnAttachedToUnit(weaponEntity, mechEntity, maxPrecisionAberration);
                _weaponHandler.OnAutoshotEnabled(weaponEntity);
            }
        }

        public void RemoveWeaponsBotControlComponents(Entity mechEntity)
        {
            foreach (var weaponEntity in _weaponsManager.GetNextGroupWeapon(mechEntity, MechWeaponGroup.Primary))
            {
                _weaponHandler.OnDetachedFromUnit(weaponEntity, mechEntity);
                _weaponHandler.OnAutoshotDisabled(weaponEntity);
            }
        }

        public float CalculateMechAttackRange(Entity mechEntity)
        {
            var primaryWeaponsCount = 0;
            var minRange = float.MaxValue;
            foreach (var weaponEntity in _weaponsManager.GetNextGroupWeapon(mechEntity, MechWeaponGroup.Primary))
            {
                primaryWeaponsCount++;
                var range = _weaponRanges.Get(weaponEntity).RecommendedRange;
                minRange = math.min(minRange, range);
            }

#if UNITY_EDITOR
            if (primaryWeaponsCount == 0)
                UnityEngine.Debug.LogError("mech have no primary weapons");

            if (minRange == 0f)
                UnityEngine.Debug.LogError("incorrect weapon ranges");
#endif

            return primaryWeaponsCount == 0 ? 100f : minRange;
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
