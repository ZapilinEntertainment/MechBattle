using R3;
using System;
using VContainer;
using ZE.UiService;

namespace ZE.MechBattle.Mech.UI
{

    // todo: convert to worker, bind to game state, hide windows on stop working
    public class MechEnergySystemUiInitializer : IDisposable
    {
        private readonly IDisposable _subscription;
        private readonly WindowsManager _windowsManager;
        private readonly WorkersFactory _workersFactory;
        private readonly PartitionsListManager _partitions;
        private readonly LifetimeTrackingManager _lifetimeTrackingManager;

        [Inject]
        public MechEnergySystemUiInitializer(
            SceneFlagsManager sceneFlags, 
            WindowsManager windowsManager, 
            WorkersFactory workersFactory,
            PartitionsListManager partitionsList,
            LifetimeTrackingManager lifetimeTrackingManager)
        {
            _windowsManager = windowsManager;
            _workersFactory = workersFactory;
            _partitions = partitionsList;
            _lifetimeTrackingManager = lifetimeTrackingManager;

            _subscription = sceneFlags.Subscribe<LocalPlayerVehicleAssignedFlag>(OnLocalPlayerControlsSet);
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }

        private void OnLocalPlayerControlsSet(LocalPlayerVehicleAssignedFlag flag)
        {
            var interfaceWindow = _windowsManager.GetWindow<UIMechInterfaceWindow>();
            var partitionWindowSubscription = _windowsManager
                .ShowWindowTemporarily<UIPartitionsWindow>(
                    interfaceWindow.GetParent(UIMechInterfaceWindow.MechInterfaceSubwindow.Partitions),
                    out var partitionsWindow);

            var vehicleEntity = flag.VehicleEntity;
            foreach (var partitionViewKvp in partitionsWindow.Partitions)
            {
                var key = partitionViewKvp.Key;
                if (!_partitions.TryGetPartition(vehicleEntity, key, out var partitionEntity))
                    continue;

                _workersFactory
                    .AddWorkerToEntity<UIMechPartitionViewWorker>(vehicleEntity)
                    .Start(partitionViewKvp.Value, partitionEntity);
            }

            _lifetimeTrackingManager.AddToEntityLifetime(vehicleEntity, partitionWindowSubscription);

            var reactorWindow = _windowsManager.ShowWindow<UIMechReactorWindow>(interfaceWindow.GetParent(UIMechInterfaceWindow.MechInterfaceSubwindow.Reactor));
            _workersFactory
                .AddWorkerToEntity<UIMechReactorWindowWorker>(vehicleEntity)
                .Start(vehicleEntity, reactorWindow);
        }
    }
}
