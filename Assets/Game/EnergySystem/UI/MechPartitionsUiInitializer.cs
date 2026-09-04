using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZE.UiService;

namespace ZE.MechBattle
{
    public class MechPartitionsUiInitializer : IDisposable
    {
        private readonly IDisposable _subscription;
        private readonly WindowsManager _windowsManager;
        private readonly WorkersFactory _workersFactory;

        [Inject]
        public MechPartitionsUiInitializer(SceneFlagsManager sceneFlags, WindowsManager windowsManager, WorkersFactory workersFactory)
        {
            _windowsManager = windowsManager;
            _workersFactory = workersFactory;
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

            _workersFactory
                .AddWorkerToEntity<UIMechPartitionsViewWorker>(flag.VehicleEntity)
                .Start(partitionsWindow, flag.VehicleEntity);
        }
    }
}
