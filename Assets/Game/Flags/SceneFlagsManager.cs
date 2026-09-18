using Scellecs.Morpeh;
using VContainer;
using ZE.Flags;

namespace ZE.MechBattle
{
    public class SceneFlagsManager : FlagsManager
    {
        private readonly LifetimeTrackingManager _lifetimeTrackingManager;

        [Inject]
        public SceneFlagsManager(LifetimeTrackingManager lifetimeTrackingManager)
        {
            _lifetimeTrackingManager = lifetimeTrackingManager;
        }

        public void AddFlagToEntity<T>(Entity entity, T flag) where T : IFlag =>
            _lifetimeTrackingManager.AddToEntityLifetime(entity, AddTemporalFlag(flag));
    }
}
