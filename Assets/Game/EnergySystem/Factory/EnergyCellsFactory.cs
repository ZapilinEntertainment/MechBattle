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
        private Stash<EnergyChargeComponent> _energyChargeComponents;
        private Stash<DamageToEnergyConsumptionConversionComponent> _conversionComponent;
        private Stash<NextEnergyCellComponent> _nextCells;
        private Stash<EnergyCellsGridComponent> _cellsGridComponent;

        [Inject]
        public EnergyCellsFactory(World world)
        {
            _world = world;

            _energyChargeComponents = world.GetStash<EnergyChargeComponent>();
            _conversionComponent = world.GetStash<DamageToEnergyConsumptionConversionComponent>();
            _nextCells = world.GetStash<NextEnergyCellComponent>();
            _cellsGridComponent = world.GetStash<EnergyCellsGridComponent>();
        }

        public void BuildPartitionEnergySystem(Entity partitionEntity, int cellsCount, EnergyCellConfig cellConfig)
        {
            Span<Entity> cells = stackalloc Entity[cellsCount];
            for (var i = 0; i < cellsCount; i++)
            {
                cells[i] = BuildEnergyCell(cellConfig);
            }

            for (var i = 0; i < cellsCount - 1; i++)
            {
                _nextCells.Set(cells[i], new(cells[i + 1]));
            }

            _cellsGridComponent.Set(partitionEntity, new(cells[0]));
        }

        public Entity BuildEnergyCell(EnergyCellConfig cellConfig)
        {
            var entity = _world.CreateEntity();
            _energyChargeComponents.Add(entity, new(cellConfig.EnergyCapacity));
            _conversionComponent.Add(entity, new(cellConfig.DamageToChargeLossCf));
            return entity;
        }
    
    }
}
