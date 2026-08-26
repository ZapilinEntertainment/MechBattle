using Scellecs.Morpeh;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.MechBuilding
{
    public class MechPartitionBuilder
    {
        public IPartitionsList PartitionsList { get; private set; }

        private Entity _mechEntity;
        private IReadOnlyDictionary<string, MechPartSettings> _partSettings;
        private IReadOnlyDictionary<ViewPartKey, Entity> _constructedBits;        

        private readonly PartitionsListManager _partitionsManager;
        private readonly MechPartitionFactory _partitionFactory;
        private readonly Stash<PartitionsRootTag> _partitionRoots;
        

        public MechPartitionBuilder(
            PartitionsListManager partitionsList,
            World world,
            MechPartitionFactory mechPartitionFactory)
        {
            _partitionsManager = partitionsList;
            _partitionFactory = mechPartitionFactory;
            _partitionRoots = world.GetStash<PartitionsRootTag>();
        }

        public void BuildAllPartitions(
            Entity mechEntity,
            MechBitsBuilder mechBuilder,
            IReadOnlyList<MechPartitionConfig> partitionConfigs)
        {
            _mechEntity = mechEntity;
            _constructedBits = mechBuilder.ConstructedParts;

            foreach (var config in partitionConfigs)
            {
                BuildPartition(config);
            }

            PartitionsList = _partitionsManager.GetPartitionsList(_mechEntity);

            _partitionRoots.Add(_mechEntity);
        }

        private Entity BuildPartition(MechPartitionConfig config)
        {
            var rootKey = config.RootPartKey;
            var key = config.Key;

            if (!TryGetParentEntity(rootKey, out var parentEntity))
                throw new System.Exception($"required root {rootKey} for {key} was not constructed");

            var entity = _partitionFactory.CreatePartition(key, _mechEntity, parentEntity, config.AttachProtocol);
            _partitionsManager.AddPartitionEntity(_mechEntity, key, entity);
            return entity;
        }

        private bool TryGetParentEntity(ViewPartKey rootKey, out Entity parentEntity)
        {
            if (!rootKey.IsValid)
            {
                parentEntity = _mechEntity;
            }
            else
            {
                if (!_constructedBits.TryGetValue(rootKey, out parentEntity))
                {
                    parentEntity = default;
                    return false;
                }                                     
            }

            return true;
        }
    }
}
