using System;
using UnityEngine;
using ZE.Workers;
using R3;
using VContainer.Unity;

namespace ZE.MechBattle
{
    public class AimWorker : Worker, ITargetDesignator
    {
        private Camera _camera;

        public AimWorker(CameraController cameraController) 
        {
            _camera = cameraController.Camera;
        }

        public ReadOnlyReactiveProperty<TargetData> TargetDataProperty => _targetDataProperty;
        private ReactiveProperty<TargetData> _targetDataProperty;

        public override void Start()
        {
            _targetDataProperty = new ReactiveProperty<TargetData>().AddTo(CompositeDisposable);
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
    }
}
