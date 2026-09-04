using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechPartitionFactory
    {
        private readonly World _world;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;
        private readonly RepairFeatureApplier _repairApplier;
        private readonly Stash<MechPartitionComponent> _partitionComponents;

        [Inject]
        public MechPartitionFactory(
            World world, 
            ParentingRelationsApplier parentingRelationsApplier,
            RepairFeatureApplier repairFeatureApplier)
        {
            _world = world;
            _parentingRelationsApplier = parentingRelationsApplier;
            _repairApplier = repairFeatureApplier;
            _partitionComponents = _world.GetStash<MechPartitionComponent>();
        }

        public Entity CreatePartition(MechPartitionKey key, Entity mechEntity, Entity parentEntity, ViewPartAttachmentProtocol attachmentProtocol)
        {
            var entity = _world.CreateEntity();
            _parentingRelationsApplier.Apply(new()
            {
                ChildEntity = entity,
                ParentEntity = parentEntity,
                LocalPos = attachmentProtocol.LocalPosition,
                LocalRot = attachmentProtocol.LocalRotation,
            });

            _partitionComponents.Add(entity, new(mechEntity, key));
            _repairApplier.ApplyOnRepairable(entity, mechEntity, new(RepairableType.MechPart, (int)key.Type));

            return entity;
        }
    
    }
}
