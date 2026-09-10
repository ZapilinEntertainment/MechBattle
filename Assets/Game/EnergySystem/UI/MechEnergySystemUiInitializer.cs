using System;
using VContainer;
using ZE.UiService;

namespace ZE.MechBattle
{

    // todo: convert to worker, bind to game state, hide windows on stop working
    public class MechEnergySystemUiInitializer : IDisposable
    {
        private readonly IDisposable _subscription;
        private readonly WindowsManager _windowsManager;
        private readonly WorkersFactory _workersFactory;
        private readonly PartitionsListManager _partitions;

        [Inject]
        public MechEnergySystemUiInitializer(
            SceneFlagsManager sceneFlags, 
            WindowsManager windowsManager, 
            WorkersFactory workersFactory,
            PartitionsListManager partitionsList)
        {
            _windowsManager = windowsManager;
            _workersFactory = workersFactory;
            _partitions = partitionsList;

            _subscription = sceneFlags.Subscribe<LocalPlayerMechControlsSetFlag>(OnLocalPlayerControlsSet);
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }

        private void OnLocalPlayerControlsSet(LocalPlayerMechControlsSetFlag flag)
        {
            var interfaceWindow = _windowsManager.ShowWindow<UIMechInterfaceWindow>();
            var partitionsWindow = _windowsManager.ShowWindow<UIPartitionsWindow>(interfaceWindow.GetParent(UIMechInterfaceWindow.MechInterfaceSubwindow.Partitions));

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


            var reactorWindow = _windowsManager.ShowWindow<UIMechReactorWindow>();
            _workersFactory
                .AddWorkerToEntity<UIMechReactorWindowWorker>(vehicleEntity)
                .Start(vehicleEntity, reactorWindow);
        }
    }
}
