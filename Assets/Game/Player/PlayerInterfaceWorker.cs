using UnityEngine;
using ZE.Workers;
using ZE.UiService;
using R3;

namespace ZE.MechBattle.UI
{
    public class PlayerInterfaceWorker : Worker
    {
        private readonly SessionData _sessionData;
        private readonly WindowsManager _windows;
        private readonly CameraController _cameraController;
        private readonly UiRoot _uiRoot;
        private readonly IUILinesParent _linesParent;

        private UIAimWindow _aimWindow;

        public PlayerInterfaceWorker(SessionData sessionData, WindowsManager windows, CameraController cameraController, IUILinesParent linesParent)
        {
            _sessionData = sessionData;
            _windows = windows;
            _cameraController = cameraController;
            _linesParent = linesParent;
        }

        public override void Start()
        {
            base.Start();
            _aimWindow = _windows.ShowWindow<UIAimWindow>();
            var weapons = _sessionData
                .LocalPlayer
                .MechController
               .GetWeapons();

            var camera = _cameraController.Camera;
            foreach (var weapon in weapons)
            {
                if (weapon.ShowInterfaceAim)
                {
                    // todo: pooling and release
                    var marker = GameObject.Instantiate(_aimWindow.AimTrackerPrefab, _aimWindow.MarkersHost);
                    marker.TrackWeapon(camera, weapon, _linesParent);
                }
            }
        }

        public override void Dispose()
        {
            _windows.HideWindow(_aimWindow);
            base.Dispose();
        }

        private void OnTargetDataChanged()
        {

        }
    }
}
