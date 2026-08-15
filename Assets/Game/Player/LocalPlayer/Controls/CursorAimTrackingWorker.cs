using R3;
using UnityEngine;
using ZE.Workers;

namespace ZE.MechBattle
{
    public class CursorAimTrackingWorker : Worker, ITargetDesignator
    {
        private Camera _camera;

        public CursorAimTrackingWorker(CameraController cameraController) 
        {
            _camera = cameraController.Camera;
        }

        public ReadOnlyReactiveProperty<TargetData> TargetDataProperty => _targetDataProperty;
        public TargetData CurrentTargetData => _targetDataProperty.Value;
        private ReactiveProperty<TargetData> _targetDataProperty = new();

        public override void Start()
        {
            if (WorkerStatus == Status.Working)
                return;

            base.Start();
            Observable.EveryUpdate().Subscribe(_ => Tick()).AddTo(CompositeDisposable);
        }

        public void Tick()
        {
            if (WorkerStatus != Status.Working)
                return;
            var cursorPosition = Input.mousePosition;
            var ray = _camera.ScreenPointToRay(cursorPosition);
            if (Physics.Raycast(ray, maxDistance: GameConstants.AIM_RAY_LENGTH, layerMask: LayerConstants.AimCastMask, hitInfo: out var hitInfo))
            {
                _targetDataProperty.Value = new(hitInfo.point);
            }
            else
            {
                _targetDataProperty.Value = new(ray.GetPoint(GameConstants.AIM_RAY_LENGTH));
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _targetDataProperty.Dispose();
        }
    }
}
