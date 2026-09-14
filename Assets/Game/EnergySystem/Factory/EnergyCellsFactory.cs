using Scellecs.Morpeh;
using System;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class EnergyCellsFactory
    {
        private readonly World _world;
        private readonly RepairFeatureApplier _repairApplier;

        private readonly Stash<EnergyChargeComponent> _energyChargeComponents;
        private readonly Stash<DamageToEnergyConsumptionConversionComponent> _conversionComponent;
        private readonly Stash<NextEnergyCellComponent> _nextCells;
        private readonly Stash<EnergyCellsGridComponent> _cellsGridComponent;
        private readonly Stash<HealthComponent> _healthComponent;
        private readonly Stash<EnergySourceComponent> _sourceComponents;

        

        [Inject]
        public EnergyCellsFactory(World world, RepairFeatureApplier repairFeatureApplier)
        {
            _world = world;
            _repairApplier = repairFeatureApplier;

            _energyChargeComponents = world.GetStash<EnergyChargeComponent>();
            _conversionComponent = world.GetStash<DamageToEnergyConsumptionConversionComponent>();
            _nextCells = world.GetStash<NextEnergyCellComponent>();
            _cellsGridComponent = world.GetStash<EnergyCellsGridComponent>();
            _healthComponent = world.GetStash<HealthComponent>();
            _sourceComponents = world.GetStash<EnergySourceComponent>();
        }

        public void BuildPartitionEnergySystem(Entity mechEntity, Entity partitionEntity, Entity reactorEntity, int cellsCount, EnergyCellConfig cellConfig)
        {
            Span<Entity> cells = stackalloc Entity[cellsCount];
            for (var i = 0; i < cellsCount; i++)
            {
                var cell = BuildEnergyCell(cellConfig, reactorEntity);
                _repairApplier.ApplyOnRepairable(cell, mechEntity, new(RepairableType.EnergyCell, i));
                cells[i] = cell;                
            }

            for (var i = 0; i < cellsCount - 1; i++)
            {
                _nextCells.Set(cells[i], new(cells[i + 1]));
                //UnityEngine.Debug.Log($"{i}/{cellsCount} : {cells[i].Id} -> {cells[i+1].Id}");
            }


            _cellsGridComponent.Set(partitionEntity, new(cells[0], cellsCount));
            _sourceComponents.Set(partitionEntity, new(reactorEntity));
        }

        public Entity BuildEnergyCell(EnergyCellConfig cellConfig, Entity reactorEntity)
        {
            var entity = _world.CreateEntity();
            _energyChargeComponents.Add(entity, new(cellConfig.EnergyCapacity));            
            _healthComponent.Add(entity, new(cellConfig.HealthPoints));
            _conversionComponent.Add(entity, new(cellConfig.DamageToChargeLossCf));
            _sourceComponents.Add(entity, new(reactorEntity));
            return entity;
        }
    
    }
}
