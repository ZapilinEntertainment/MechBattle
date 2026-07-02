using UnityEngine;
using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle.Ecs
{
    public class InitialDelayApplier
    {
        private readonly Stash<InitialDelayComponent> _stash;

        [Inject]
        public InitialDelayApplier(World world)
        {
            _stash = world.GetStash<InitialDelayComponent>();
        }

        public void ApplyInitialDelay(Entity entity, float value)
        {
            var stopTime = Time.time + value;
            _stash.Set(entity, new(stopTime));
        }

    }
}
