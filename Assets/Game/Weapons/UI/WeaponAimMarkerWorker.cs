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
        private readonly MechWeaponsHandler _mechWeaponsHandler;
        private readonly WeaponHandler _weaponHandler;
        private readonly IMechUIElementsVisibilityController _elementsVisibilityController;

        private Entity _weaponEntity;
        private Entity _mechEntity;
        private Entity _aimingEntity;
        private UIWeaponAimMarker _aimMarker;
        private TargetData _targetData;

        [Inject]
        public WeaponAimMarkerWorker(
            TransformAspectHandler transformAspectHandler, 
            AimCaster aimCaster,
            ICursorAimTracker cursorTracker,
            WeaponTargetMarkerFactory weaponTargetMarkerFactory,
            World world,
            MechWeaponsHandler mechWeaponsHandler,
            WeaponHandler weaponHandler,
            IMechUIElementsVisibilityController elementsVisibilityController)
        {
            _transformAspectHandler = transformAspectHandler;
            _aimCaster = aimCaster;
            _cursorTracker = cursorTracker;
            _markerFactory = weaponTargetMarkerFactory;
            _world = world;
            _mechWeaponsHandler = mechWeaponsHandler;
            _weaponHandler = weaponHandler;
            _elementsVisibilityController = elementsVisibilityController;
        }

        public void Start(Entity weaponEntity, Entity mechEntity, WeaponAimMarkerDisplayProtocol displayProtocol)
        {
            _weaponEntity = weaponEntity;
            _mechEntity = mechEntity;
            _aimingEntity = _weaponHandler.GetWeaponsAimingEntity(weaponEntity);

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

            _aimMarker.Setup(displayProtocol);
            _elementsVisibilityController
                .VisibilityFlagsProperty
                .Select(visibility => visibility.HasFlag(MechInterfaceElementsVisibilityFlags.MainGunAim))
                .Subscribe(_aimMarker.SetVisibility)
                .AddTo(CompositeDisposable);
        }

        public override void Dispose()
        {
            base.Dispose();
            _aimMarker.Dispose();
        }

        private void Update(Unit unit)
        {
            var gunPoint = _transformAspectHandler.GetPoint(_aimingEntity);
            _aimCaster.TryGetRayEndScreenPos(_targetData, gunPoint, out var screenPos);

            var loadingPc = _mechWeaponsHandler.GetWeaponLoadingProgress(_weaponEntity);
            _aimMarker.UpdateData(new()
            {
                ScreenPos = screenPos,
                GunLoadingProgress = loadingPc,
                EnergyChargePc = _mechWeaponsHandler.GetWeaponEnergySatisfactionPc(_mechEntity, _weaponEntity)
            });
        }
    }
}
