using Scellecs.Morpeh;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechPartitionBuilder
    {
        public readonly Entity MechEntity;
        private readonly MechPartitionFactory _partitionsFactory;
        private readonly PartitionsListManager _partitionsList;
        private readonly IReadOnlyDictionary<string, MechPartSettings> _partSettings;
        private readonly IReadOnlyDictionary<string, MechPartsBuilder.PartData> _constructedParts;
        private readonly Stash<PartitionsRootTag> _partitionRoots;

        public MechPartitionBuilder(
            Entity mechEntity, 
            MechConfig mechConfig, 
            MechPartitionFactory mechPartitionFactory,
            MechPartsBuilder mechBuilder,
            PartitionsListManager partitionsList,
            World world)
        {
            MechEntity = mechEntity;
            _partitionsFactory = mechPartitionFactory;
            _partitionsList = partitionsList;

            _partSettings = mechConfig.MechPartSettings;
            _constructedParts = mechBuilder.ConstructedParts;

            _partitionRoots = world.GetStash<PartitionsRootTag>();
        }

        public void BuildAllPartitions()
        {
            BuildPartition( MechPartitionKey.Center, MechConstants.UPPER_PART_ID);
            BuildPartition(MechPartitionKey.LeftArm, MechConstants.LEFT_ARM_PARTITION_ID);
            BuildPartition(MechPartitionKey.RightArm, MechConstants.RIGHT_ARM_PARTITION_ID);
            BuildPartition(MechPartitionKey.LeftLeg, MechConstants.LEFT_LEG_PARTITION_ID);
            BuildPartition(MechPartitionKey.RightLeg, MechConstants.RIGHT_LEG_PARTITION_ID);

            _partitionRoots.Add(MechEntity);
        }

        private Entity BuildPartition(MechPartitionKey key, string settingsKey)
        {
            if (!_partSettings.TryGetValue(settingsKey, out var settings))
                throw new System.Exception(settingsKey + " settings not found");

            if (!TryGetParentEntity(settings.Root, out var parentEntity))
                throw new System.Exception($"required root {settings.Root} for {settingsKey} was not constructed");

            var entity = _partitionsFactory.CreatePartition(key, MechEntity, parentEntity, settings.AttachProtocol);
            _partitionsList.AddPartitionEntity(MechEntity, key, entity);
            return entity;
        }

        private bool TryGetParentEntity(string rootId, out Entity parentEntity)
        {
            if (string.IsNullOrEmpty(rootId))
            {
                parentEntity = MechEntity;
            }
            else
            {
                if (!_constructedParts.TryGetValue(rootId, out var partData))
                {
                    parentEntity = default;
                    return false;
                }                                     
                parentEntity = partData.Entity;
            }

            return true;
        }
    }
}
