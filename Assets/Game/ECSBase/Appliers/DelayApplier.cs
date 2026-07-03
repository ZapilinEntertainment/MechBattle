using UnityEngine;
using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle.Ecs
{
    public class DelayApplier
    {
        private readonly Stash<InitialDelayComponent> _initialDelays;
        private readonly Stash<EntityDestructionDelayComponent> _entityDestructionDelays;

        [Inject]
        public DelayApplier(World world)
        {
            _initialDelays = world.GetStash<InitialDelayComponent>();
            _entityDestructionDelays = world.GetStash<EntityDestructionDelayComponent>();
        }

        public void ApplyInitialDelay(Entity entity, float delay)
        {
            var stopTime = Time.time + delay;
            _initialDelays.Set(entity, new(stopTime));
        }

        public void ApplyDestructionDelay(Entity entity, float delay)
        {
            var stopTime = Time.time + delay;
            _entityDestructionDelays.Set(entity, new(stopTime));
        }

        public bool HasDestructionDelay(Entity entity) => _entityDestructionDelays.Has(entity);

    }
}
