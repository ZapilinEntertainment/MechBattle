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
        private readonly ICursorAimTracker _cursorTracker;
        private readonly WeaponTargetMarkerFactory _markerFactory;
        private readonly World _world;

        private Entity _weaponEntity;
        private UIWeaponAimMarker _aimMarker;
        private TargetData _targetData;

        [Inject]
        public WeaponAimMarkerWorker(
            TransformAspectHandler transformAspectHandler, 
            AimCaster aimCaster,
            ICursorAimTracker cursorTracker,
            WeaponTargetMarkerFactory weaponTargetMarkerFactory,
            World world)
        {
            _transformAspectHandler = transformAspectHandler;
            _aimCaster = aimCaster;
            _cursorTracker = cursorTracker;
            _markerFactory = weaponTargetMarkerFactory;
            _world = world;
        }

        public void Start(Entity weaponEntity)
        {
            _weaponEntity = weaponEntity;
            _aimMarker = _markerFactory.Create();
            base.Start();

            _cursorTracker
                .TargetDataProperty
                .Subscribe(targetData => _targetData = targetData)
                .AddTo(CompositeDisposable);

            Observable.EveryUpdate()
                .Where(_ => !_world.IsDisposed(_weaponEntity))
                .Subscribe(Update)
                .AddTo(CompositeDisposable);

            _aimMarker.SetVisibility(true);
        }

        public override void Dispose()
        {
            base.Dispose();
            _aimMarker.Dispose();
        }

        private void Update(Unit unit)
        {
            var gunPoint = _transformAspectHandler.GetPoint(_weaponEntity);
            _aimCaster.TryGetRayEndScreenPos(_targetData, gunPoint, out var screenPos);
            _aimMarker.SetPosition(screenPos);
        }
    }
}
