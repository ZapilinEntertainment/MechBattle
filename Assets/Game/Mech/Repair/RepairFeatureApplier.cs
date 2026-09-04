using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class RepairFeatureApplier
    {
        private readonly Stash<RepairTeamsComponent> _teams;
        private readonly Stash<RepairSpeedComponent> _repairSpeed;
        private readonly Stash<RepairableComponent> _repairable;

        [Inject]
        public RepairFeatureApplier(World world)
        {
            _teams = world.GetStash<RepairTeamsComponent>();
            _repairSpeed = world.GetStash<RepairSpeedComponent>();
            _repairable = world.GetStash<RepairableComponent>();
        }

        // for entity that will do the repairs
        public void ApplyOnRepairProduceEntity(Entity mechEntity, int repairTeams, float repairSpeed)
        {
            _teams.Set(mechEntity, new(repairTeams));
            _repairSpeed.Set(mechEntity, new() { Value = repairSpeed });
        }

        // for potential repair receiver
        public void ApplyOnRepairable(Entity entity, Entity repairTeamsEntity, RepairPriority priority)
        {
            _repairable.Set(entity, new(repairTeamsEntity, priority));
        }
    
    }
}
