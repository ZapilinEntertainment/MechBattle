using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechPartitionFactory
    {
        private readonly World _world;
        private readonly PartitionsList _partitionsList;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;
        private readonly Stash<PartitionsRootTag> _partitionRoots;

        [Inject]
        public MechPartitionFactory(World world, PartitionsList partitionsList, ParentingRelationsApplier parentingRelationsApplier)
        {
            _world = world;
            _partitionsList = partitionsList;
            _parentingRelationsApplier = parentingRelationsApplier;

            _partitionRoots = _world.GetStash<PartitionsRootTag>();
        }

        public void CreatePartitions(Entity mechEntity, MechConfig mechConfig)
        {
            CollidersConfiguration collidersConfig =  default; //mechConfig.PartitionCollidersConfig;
            CreatePartition(mechEntity, MechPartitionKey.Center, collidersConfig);
            CreatePartition(mechEntity, MechPartitionKey.LeftArm, collidersConfig);
            CreatePartition(mechEntity, MechPartitionKey.RightArm, collidersConfig);
            CreatePartition(mechEntity, MechPartitionKey.LeftLeg, collidersConfig);
            CreatePartition(mechEntity, MechPartitionKey.RightLeg, collidersConfig);

            _partitionRoots.Add(mechEntity);
        }

        private void CreatePartition(Entity mechEntity, MechPartitionKey key, CollidersConfiguration collidersConfig)
        {
            var partitionEntity = _world.CreateEntity();
            //_parentingRelationsApplier.Apply(new()
            //{
            //    AwaitParentViewComponent = false,
            //    ChildEntity = partitionEntity,
            //    ParentEntity = mechEntity,
            //    d
            //})

            if (collidersConfig.TryGetColliderSetupInfo(MechPartitionKey.Center.ToString(), out var setupInfo))
            {
               
            }
            else
            {
                UnityEngine.Debug.LogError($"{key} partition collider info not found");
            }
        }
    
    }
}
