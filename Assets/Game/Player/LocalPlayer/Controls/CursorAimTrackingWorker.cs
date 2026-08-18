using R3;
using UnityEngine;
using VContainer;
using ZE.Workers;

namespace ZE.MechBattle
{
    public class CursorAimTrackingWorker : Worker, ICursorAimTracker
    {
        public ReadOnlyReactiveProperty<TargetData> TargetDataProperty => _targetDataProperty;
        public TargetData CurrentTargetData => _targetDataProperty.Value;
        private ReactiveProperty<TargetData> _targetDataProperty = new();
        private readonly AimCaster _aimCaster;

        [Inject]
        public CursorAimTrackingWorker(AimCaster aimCaster)
        {
            _aimCaster = aimCaster;
        }

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
            _aimCaster.TryCastScreenPointRay(cursorPosition, out var hitPos);
            _targetDataProperty.Value = new(hitPos);
        }

        public override void Dispose()
        {
            base.Dispose();
            _targetDataProperty.Dispose();
        }
    }
}
