using R3;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class LocalPlayerInitializer : EntityComponentTrackerBase<ControlledVehicleComponent>
    {
        private readonly SceneFlagsManager _sceneFlags;
        private readonly LifetimeTrackingManager _lifetimeTrackingManager;
        private readonly Stash<ViewLoadRequestTag> _viewLoadRequests;
        private readonly PlayerUiInitializer _uiInitializer;
        private Entity _vehicleEntity;

        public LocalPlayerInitializer(
            World world, 
            SceneFlagsManager sceneFlags, 
            LifetimeTrackingManager lifetimeTrackingManager,
            PlayerUiInitializer playerUiInitializer) : base(world)
        {
            _sceneFlags = sceneFlags;
            _lifetimeTrackingManager = lifetimeTrackingManager;
            _uiInitializer = playerUiInitializer;
            _viewLoadRequests = world.GetStash<ViewLoadRequestTag>();
        }

        protected override bool IsStashConditionMatch()
        {
            var controlledVehicleComponent = Stash.Get(Entity, out var haveControlledEntity);
            if (!haveControlledEntity)
                return false;

            _vehicleEntity = controlledVehicleComponent.Entity;
            return !_viewLoadRequests.Has(_vehicleEntity);
        }

        protected override void OnConditionMatched()
        {
            var lifetimeObject = _lifetimeTrackingManager.GetEntityLifetimeObject(Entity);
            var flag = new LocalPlayerViewInstancedFlag(Entity, _vehicleEntity);
            lifetimeObject.Add(_sceneFlags.AddTemporalFlag(flag));

            //_uiInitializer.Activate(Entity);
        }
    }
}
