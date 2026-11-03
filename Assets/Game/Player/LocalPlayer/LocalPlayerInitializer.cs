using Scellecs.Morpeh;
using VContainer;
using VContainer.Unity;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class LocalPlayerInitializer : EntityComponentTrackerBase<ControlledVehicleComponent>, IInitializable
    {
        private readonly SceneFlagsManager _sceneFlags;
        private readonly LifetimeTrackingManager _lifetimeTrackingManager;
        private readonly Stash<ViewLoadRequestTag> _viewLoadRequests;
        private readonly WorkersFactory _workersFactory;

        private readonly LevelSettingsObject _levelSettings;
        private readonly FactionHandler _factionHandler;
        private readonly MechCreateRequestsFactory _mechCreateRequestsFactory;
        private Entity _vehicleEntity;

        [Inject]
        public LocalPlayerInitializer(
            World world, 
            SceneFlagsManager sceneFlags, 
            LifetimeTrackingManager lifetimeTrackingManager,
            WorkersFactory workersFactory,

            LevelSettingsObject levelObject,
            FactionHandler factionHandler,
            MechCreateRequestsFactory mechCreateRequestsFactory) : base(world)
        {
            _sceneFlags = sceneFlags;
            _lifetimeTrackingManager = lifetimeTrackingManager;
            _workersFactory = workersFactory;

            _factionHandler = factionHandler;
            _levelSettings = levelObject;
            _mechCreateRequestsFactory = mechCreateRequestsFactory;

            _viewLoadRequests = world.GetStash<ViewLoadRequestTag>();
        }

        public void Initialize()
        {
            var localPlayerSpawn = _levelSettings.GetSpawnPoint(GameConstants.LOCAL_PLAYER_KEY).ToRigidTransform();
            var localPlayerEntity = _factionHandler.GetLocalPlayerEntity();

            // create mech for player
            _mechCreateRequestsFactory.CreateRequest(new(_factionHandler.GetLocalPlayerKey(), localPlayerSpawn.pos, localPlayerSpawn.rot, directControl: true));

            // wait until player have controls over mech
            StartTracking(localPlayerEntity);
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
            var flag = new LocalPlayerVehicleAssignedFlag(Entity, _vehicleEntity);
            _lifetimeTrackingManager.AddToEntityLifetime(Entity, _sceneFlags.AddTemporalFlag(flag));
            
            _workersFactory
                .AddWorkerToEntity<MechHeadRotationWorker>(_vehicleEntity)
                .Start();
        }
    }
}
