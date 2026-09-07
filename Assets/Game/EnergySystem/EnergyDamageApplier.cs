using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Energy
{
    public class EnergyDamageApplier
    {
        private struct EnergyCell : IComparable<EnergyCell>
        {
            public readonly Entity Entity;
            public float Charge;
            public float Health;

            public EnergyCell(Entity entity, float charge, float health)
            {
                Entity = entity;
                Charge = charge;
                Health = health;
            }

            public int CompareTo(EnergyCell other)
            {
                var chargeComparison = -Charge.CompareTo(other.Charge);
                if (chargeComparison == 0)
                    return -Health.CompareTo(other.Health);

                return chargeComparison;
            }
        }

        private readonly Stash<EnergyCellsGridComponent> _energyGrid;
        private readonly Stash<NextEnergyCellComponent> _nextCell;
        private readonly Stash<DamageToEnergyConsumptionConversionComponent> _conversionCfs;
        private readonly Stash<EnergyChargeComponent> _energyCharge;
        private readonly Stash<RepairRequiredTag> _repairRequiredTags;
        private readonly Stash<HealthComponent> _healthComponents;
        private readonly Stash<DamageReceivedComponent> _damageReceived;
        private readonly List<EnergyCell> _cellsList = new(capacity : 10);

        [Inject]
        public EnergyDamageApplier(World world)
        {
            _energyGrid = world.GetStash<EnergyCellsGridComponent>();
            _nextCell = world.GetStash<NextEnergyCellComponent>();
            _conversionCfs = world.GetStash<DamageToEnergyConsumptionConversionComponent>();
            _energyCharge = world.GetStash<EnergyChargeComponent>();
            _repairRequiredTags = world.GetStash<RepairRequiredTag>();
            _healthComponents = world.GetStash<HealthComponent>();
            _damageReceived = world.GetStash<DamageReceivedComponent>();
        }

        public float ApplyDamageToEnergyGrid(Entity receiver, float damageVolume, Entity maxDamageProducer)
        {
            var gridComponent = _energyGrid.Get(receiver);
            var energyCellEntity = gridComponent.FirstCellEntity;

            // prepare cell data list for sorting
            foreach (var cellEntity in new EnergyCellsEnumerator(_nextCell, energyCellEntity))
            {
                var health = _healthComponents.Get(cellEntity).CurrentValue;
                if (health == 0f)
                    continue;
                var charge = _energyCharge.Get(cellEntity).Value;                
                _cellsList.Add(new (cellEntity, charge, health));
            }

            // if every cell is broken
            if (_cellsList.Count == 0)
                return damageVolume;

            // energy spending:
            _cellsList.Sort();
            for (var i = 0; i < _cellsList.Count;i++)
            {
                var cellData = _cellsList[i];
                if (cellData.Charge == 0f)
                    continue;

                var conversionCf = _conversionCfs.Get(cellData.Entity).Coefficient;
                var convertedDamage = damageVolume * conversionCf;

                ref var chargeComponent = ref _energyCharge.Get(cellData.Entity);
                if (chargeComponent.Value < convertedDamage)
                {
                    damageVolume -= chargeComponent.Value / conversionCf;
                    chargeComponent.Value = 0f;
                    //UnityEngine.Debug.Log($"energy cell {cellData.Entity.Id} exhausted");
                }
                else
                {
                    chargeComponent.Value -= convertedDamage;
                    damageVolume = 0f;
                    break;
                }
            }
            
            // health spending
            if (damageVolume != 0f)
            {
                _cellsList.Sort();

                for (var i = 0; i < _cellsList.Count; i++)
                {
                    var cellData = _cellsList[i];
                    _repairRequiredTags.Set(cellData.Entity);

                    ref var healthComponent = ref _healthComponents.Get(cellData.Entity);
                    if (healthComponent.CurrentValue < damageVolume)
                    {
                        damageVolume -= healthComponent.CurrentValue;
                        healthComponent.CurrentValue = 0f;
                        _damageReceived.Set(cellData.Entity);
                        //UnityEngine.Debug.Log($"energy cell {cellData.Entity.Id} completely destroyed");
                    }
                    else
                    {
                        healthComponent.CurrentValue -= damageVolume;
                        damageVolume = 0f;
                        _damageReceived.Set(cellData.Entity);
                        break;
                    }
                }
            }


            _cellsList.Clear();
            return damageVolume;
        }   
    }
}
