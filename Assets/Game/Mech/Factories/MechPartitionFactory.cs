using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechPartitionFactory
    {
        private readonly World _world;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;
        private readonly Stash<MechPartitionComponent> _partitionComponents;

        [Inject]
        public MechPartitionFactory(
            World world, 
            ParentingRelationsApplier parentingRelationsApplier)
        {
            _world = world;
            _parentingRelationsApplier = parentingRelationsApplier;
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

            return entity;
        }
    
    }
}
