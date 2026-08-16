using R3;
using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.Workers;

namespace ZE.MechBattle
{
    public class WeaponAimMarkerWorker : Worker
    {
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly AimCaster _aimCaster;
        private readonly CursorAimTrackingWorker _aimTrackingWorker;
        private readonly WeaponTargetMarkerFactory _markerFactory;

        private Entity _weaponEntity;
        private UIWeaponAimMarker _aimMarker;

        [Inject]
        public WeaponAimMarkerWorker(
            TransformAspectHandler transformAspectHandler, 
            AimCaster aimCaster, 
            CursorAimTrackingWorker cursorAimTrackingWorker,
            WeaponTargetMarkerFactory weaponTargetMarkerFactory)
        {
            _transformAspectHandler = transformAspectHandler;
            _aimCaster = aimCaster;
            _aimTrackingWorker = cursorAimTrackingWorker;
            _markerFactory = weaponTargetMarkerFactory;
        }

        public void Start(Entity weaponEntity)
        {
            _weaponEntity = weaponEntity;
            _aimMarker = _markerFactory.Create();
            base.Start();

            _aimTrackingWorker
                .TargetDataProperty
                .Subscribe(OnTargetDataChanged)
                .AddTo(CompositeDisposable);            
        }

        public override void Dispose()
        {
            base.Dispose();
            _aimMarker.Dispose();
        }

        private void OnTargetDataChanged(TargetData targetData)
        {
            _aimMarker.SetVisibility(targetData.IsDefined);
            if (!targetData.IsDefined)
                return;

            var gunPoint = _transformAspectHandler.GetPoint(_weaponEntity);
            if (_aimCaster.TryGetRayEndScreenPos(targetData, gunPoint, out var screenPos))
                _aimMarker.SetPosition(screenPos);
        }
    }
}
