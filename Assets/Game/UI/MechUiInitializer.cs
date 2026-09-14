using R3;
using Scellecs.Morpeh;
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZE.UiService;

namespace ZE.MechBattle.Mech.UI
{
    public class MechUiInitializer : IDisposable, IInitializable
    {
        private readonly CompositeDisposable _compositeDisposable = new();
        private readonly WindowsManager _windowsManager;
        private readonly IWeaponsManager _weaponsManager;
        private readonly WorkersFactory _workersFactory;
        private readonly SceneFlagsManager _sceneFlags;

        [Inject]
        public MechUiInitializer(
            SceneFlagsManager flags,              
            WindowsManager windowsManager,
            IWeaponsManager weaponsManager,
            WorkersFactory workersFactory)
        {
            _windowsManager = windowsManager;
            _weaponsManager = weaponsManager;
            _workersFactory = workersFactory;
            _sceneFlags = flags;
                      
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        public void Initialize()
        {
            _sceneFlags
                .Subscribe<PlayerCameraSetFlag>(OnPlayerCameraSet)
                .AddTo(_compositeDisposable);
        }

        private void OnPlayerCameraSet(PlayerCameraSetFlag flag)
        {

            _windowsManager.ShowWindow<UIMechInterfaceWindow>();
            var interfaceWorker = _workersFactory.CreateWorker<UIMechInterfaceWorker>();
            interfaceWorker.AddTo(_compositeDisposable);
            interfaceWorker.Start();


            var aimWindow = _windowsManager.ShowWindow<UIAimWindow>();

            var mechEntity = flag.VehicleEntity;
            var colors = new Color[2] { Color.red, Color.blue };
            var i = 0;
            foreach (var weaponEntity in _weaponsManager.GetNextGroupWeapon(mechEntity, MechWeaponGroup.Primary))
            {
                var color = colors[i];
                var markerWorker = _workersFactory.AddWorkerToEntity<WeaponAimMarkerWorker>(mechEntity);
                markerWorker.Start(weaponEntity, mechEntity, new()
                {
                    Color = color,
                    Level = i
                });

                i = (i + 1) % colors.Length;
            }


            if (_weaponsManager.TryGetWeapon(mechEntity, new MechWeaponKey(MechWeaponGroup.Eyes, 0), out var laserEyesWeaponEntity))
            {
                var laserEyesWindow = _windowsManager.ShowWindow<UILaserEyesWindow>(aimWindow.transform);
                var laserEyesUiWorker = _workersFactory.AddWorkerToEntity<UILaserEyesPanelWorker>(mechEntity);
                laserEyesUiWorker.Start(laserEyesWeaponEntity, laserEyesWindow);
            }
        }
    }
}
