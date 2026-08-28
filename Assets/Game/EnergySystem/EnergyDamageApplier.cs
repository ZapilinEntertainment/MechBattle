using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Energy
{
    public class EnergyDamageApplier
    {
        private readonly Stash<EnergyCellsGridComponent> _cells;
        private readonly Stash<NextEnergyCellComponent> _nextCell;
        private readonly Stash<DamageToEnergyConsumptionConversionComponent> _conversionCfs;
        private readonly Stash<EnergyChargeComponent> _energyCharge;
        private readonly Stash<RepairRequiredTag> _repairRequiredTags;
        private readonly Stash<HealthComponent> _healthComponents;
        private readonly List<Entity> _cellsList = new(capacity : 10);

        [Inject]
        public EnergyDamageApplier(World world)
        {
            _cells = world.GetStash<EnergyCellsGridComponent>();
            _nextCell = world.GetStash<NextEnergyCellComponent>();
            _conversionCfs = world.GetStash<DamageToEnergyConsumptionConversionComponent>();
            _energyCharge = world.GetStash<EnergyChargeComponent>();
            _repairRequiredTags = world.GetStash<RepairRequiredTag>();
            _healthComponents = world.GetStash<HealthComponent>();
        }

        public float ApplyDamageToEnergyGrid(Entity receiver, float damageVolume, Entity maxDamageProducer)
        {
            var energyCellEntity = _cells.Get(receiver).FirstCellEntity;
            bool nextCellExists;
            do
            {
                _cellsList.Add(energyCellEntity);
                energyCellEntity = _nextCell.Get(energyCellEntity, out nextCellExists).CellEntity;
            }
            while (nextCellExists);


            var elementsCount = _cellsList.Count;
            float excessDamage = damageVolume;
            for (var i = elementsCount - 1; i > -1; i++)
            {
                var cellEntity = _cellsList[i];
                if (_repairRequiredTags.Has(cellEntity))
                    continue;

                excessDamage = ApplyDamageOnEnergyCell(cellEntity, damageVolume);
                if (excessDamage == 0f)
                    break;
            }

            _cellsList.Clear();

            return excessDamage;
        }

        public float ApplyDamageOnEnergyCell(Entity energyCellEntity, float damage)
        {
            var conversionCf = _conversionCfs.Get(energyCellEntity).Coefficient;
            if (conversionCf == 0f)
                return 0f;

            var energyDamage = damage * conversionCf;

            ref var chargeComponent = ref _energyCharge.Get(energyCellEntity);
            if (chargeComponent.Value < energyDamage)
            {
                energyDamage -= chargeComponent.Value;
                chargeComponent.Value = 0f;
            }
            else
            {
                chargeComponent.Value -= energyDamage;
                energyDamage = 0f;
                return 0f;
            }

            var excessDamage = energyDamage / conversionCf;
            ref var healthComponent = ref _healthComponents.Get(energyCellEntity);
            if (healthComponent.CurrentValue < excessDamage)
            {
                excessDamage -= healthComponent.CurrentValue;
                healthComponent.CurrentValue = 0f;
                _repairRequiredTags.Add(energyCellEntity);
                return excessDamage;
            }
            else
            {
                healthComponent.CurrentValue -= excessDamage;
                return 0f;
            }
        }
    
    }
}
