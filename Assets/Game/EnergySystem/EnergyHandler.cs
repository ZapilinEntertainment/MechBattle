using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Damage;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class EnergyHandler
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

        private struct EnergySource : IComparable<EnergySource>
        {
            public readonly Entity Entity;
            public readonly MechPartitionType Partition;
            public readonly float Charge;

            public EnergySource(Entity entity, MechPartitionType partitionType, float charge)
            {
                Entity = entity;
                Partition = partitionType;
                Charge = charge;
            }

            public int CompareTo(EnergySource other)
            {
                var chargeComparison = -Charge.CompareTo(other.Charge);
                if (chargeComparison == 0)
                    return -Partition.CompareTo(other.Partition);

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
        private readonly Stash<EnergySpentComponent> _energySpent;
        private readonly Stash<EnergySourceComponent> _energySourceComponents;
        private readonly Stash<EnergyGridModeComponent> _gridModeComponents;

        private readonly List<EnergyCell> _cellsList = new(capacity : 10);
        private readonly List<EnergySource> _energySourceList = new(capacity: 32);

        private readonly World _world;
        private readonly PartitionsListManager _partitionsListManager;
        private readonly DamageRequestsList _damageRequestsList;

        [Inject]
        public EnergyHandler(World world, PartitionsListManager partitionsListManager, DamageRequestsList damageRequestsList)
        {
            _world = world;
            _partitionsListManager = partitionsListManager;
            _damageRequestsList = damageRequestsList;

            _energyGrid = _world.GetStash<EnergyCellsGridComponent>();
            _nextCell = _world.GetStash<NextEnergyCellComponent>();
            _conversionCfs = _world.GetStash<DamageToEnergyConsumptionConversionComponent>();
            _energyCharge = _world.GetStash<EnergyChargeComponent>();
            _repairRequiredTags = _world.GetStash<RepairRequiredTag>();
            _healthComponents = _world.GetStash<HealthComponent>();
            _damageReceived = _world.GetStash<DamageReceivedComponent>();
            _energySpent = _world.GetStash<EnergySpentComponent>();

            _energySourceComponents = _world.GetStash<EnergySourceComponent>();
            _gridModeComponents = _world.GetStash<EnergyGridModeComponent>();
        }

        public bool TryTransferExcessDamageOnRepairable(Entity repairableEntity, float excessDamage)
        {
            var energySource = _energySourceComponents.Get(repairableEntity, out var sourceComponentExists);
            if (!sourceComponentExists || _world.IsDisposed(energySource.SourceEntity))
                return false;

            _damageRequestsList.Add(new(default, energySource.SourceEntity, new(DamageType.DamageTransfer, excessDamage)));
            return true;
        }

        public void WriteSpentEnergy(Entity spendingEntity, float energyVolume)
        {
            var sourceEntity = _energySourceComponents.Get(spendingEntity).SourceEntity;
            var component = _energySpent.Get(sourceEntity, out var exists);
            var volume = exists ? component.Volume + energyVolume : energyVolume;
            _energySpent.Set(sourceEntity, new() { Volume = volume });
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

        public bool TrySpendEnergyForEntity(Entity consumerEntity, float requiredEnergyVolume, out float shortage)
        {
            var energySource = _energySourceComponents.Get(consumerEntity).SourceEntity;
            var gridModeComponent = _gridModeComponents.Get(energySource);
            if (gridModeComponent.Mode != EnergyGridMode.Partitions)
                throw new System.NotImplementedException("grid mode not implemented");

            return TrySpendEnergyFromPartitions(
                gridModeComponent.Entity,
                _partitionsListManager.GetPartitionsList(gridModeComponent.Entity),
                requiredEnergyVolume,
                out shortage);
        }

        public bool TrySpendEnergyFromPartitions(Entity mechEntity, IPartitionsList partitions, float requiredEnergyVolume, out float shortage)
        {
            foreach (var partitionKvp in partitions)
            {
                var firstCellEntity = _energyGrid.Get(partitionKvp.Value).FirstCellEntity;
                foreach (var cellEntity in new EnergyCellsEnumerator(_nextCell, firstCellEntity))
                {
                    var charge = _energyCharge.Get(cellEntity).Value;
                    _energySourceList.Add(new(cellEntity, partitionKvp.Key.Type, charge));
                }
            }

            _energySourceList.Sort();
            var spent = 0f;
            foreach (var source in _energySourceList)
            {
                ref var charge = ref _energyCharge.Get(source.Entity);
                if (charge.Value > requiredEnergyVolume)
                {
                    spent += charge.Value;
                    charge.Value -= requiredEnergyVolume;
                    requiredEnergyVolume = 0f;
                    break;
                }
                else
                {
                    requiredEnergyVolume -= charge.Value;
                    spent += charge.Value;
                    charge.Value = 0f; 
                }
            }

            shortage = requiredEnergyVolume;

            WriteSpentEnergy(mechEntity, requiredEnergyVolume);

            _energySourceList.Clear();

            return shortage == 0f;
        }
    }
}
