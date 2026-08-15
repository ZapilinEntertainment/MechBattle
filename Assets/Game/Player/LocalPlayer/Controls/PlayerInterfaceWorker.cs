using UnityEngine;
using ZE.UiService;
using ZE.Workers;

namespace ZE.MechBattle.UI
{
    public class PlayerInterfaceWorker : Worker
    {
        private readonly WindowsManager _windows;
        private readonly CameraController _cameraController;
        private readonly IUILinesParent _linesParent;

        private UIAimWindow _aimWindow;

        public PlayerInterfaceWorker(WindowsManager windows, CameraController cameraController, IUILinesParent linesParent)
        {
            _windows = windows;
            _cameraController = cameraController;
            _linesParent = linesParent;
        }

        public override void Start()
        {
            base.Start();
            _aimWindow = _windows.ShowWindow<UIAimWindow>();

            //var camera = _cameraController.Camera;
            //foreach (var weapon in weapons)
            //{
            //    if (weapon.ShowInterfaceAim)
            //    {
            //        // todo: pooling and release
            //        var marker = GameObject.Instantiate(_aimWindow.AimTrackerPrefab, _aimWindow.MarkersHost);
            //        marker.TrackWeapon(camera, weapon, _linesParent);
            //    }
            //}
        }

        public override void Dispose()
        {
            _windows.HideWindow(_aimWindow);
            base.Dispose();
        }
    }
}
