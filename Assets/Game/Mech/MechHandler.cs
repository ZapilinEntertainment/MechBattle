using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechHandler
    {
        private readonly Stash<MechComponent> _mechComponents;
        private readonly AffinityHandler _affinityHandler;
        private readonly PartitionsListManager _partitionsManager;

        [Inject]
        public MechHandler(World world, AffinityHandler affinityHandler, PartitionsListManager partitionsListManager)
        {
            _mechComponents = world.GetStash<MechComponent>();
            _affinityHandler = affinityHandler;
            _partitionsManager = partitionsListManager;
        }

        public Entity GetHeadEntity(Entity mechEntity) => _mechComponents.Get(mechEntity).HeadEntity;
        public Entity GetChassisEntity(Entity mechEntity) => _mechComponents.Get(mechEntity).ChassisEntity;

        public void AssignMechPlayerAffinity(Entity mechEntity, PlayerKey playerKey)
        {
            _affinityHandler.SetEntityAffinity(mechEntity, playerKey);
            var partitions = _partitionsManager.GetPartitionsList(mechEntity);
            foreach (var partitionEntity in partitions.Entities)
            {
                _affinityHandler.SetEntityAffinity(partitionEntity, playerKey);
            }
        }

    }
}
