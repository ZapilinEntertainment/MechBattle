using R3;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PlayerInputSystem : ISystem 
    {
        public World World { get; set;}

        private readonly WorkersFactory _workersFactory;
        private readonly CompositeDisposable _compositeDisposable;
        private readonly CursorAimTrackingWorker _aimWorker;
        private readonly SceneFlagsManager _flags;
        private readonly ReactiveProperty<bool> _eyesActiveProperty = new(false);

        private bool _playerVehiclePresented = false;
        private MechControlsWorker _mechController;
        

        [Inject]
        public PlayerInputSystem(
            SceneFlagsManager flags, 
            ICursorAimTracker aimWorker,
            WorkersFactory workersFactory)
        {
            _workersFactory = workersFactory;
            _flags = flags;

            _compositeDisposable = new();
            _flags
                .Subscribe<LocalPlayerVehicleAssignedFlag>(OnPlayerViewLoaded)
                .AddTo(_compositeDisposable);
            _flags
                .Subscribe<LocalPlayerVehicleAssignedFlag>(flagActive => _playerVehiclePresented = flagActive)
                .AddTo(_compositeDisposable);

            _aimWorker = aimWorker as CursorAimTrackingWorker;
        }

        public void OnAwake() 
        {
            _aimWorker.Start();
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_playerVehiclePresented)
                return;

            // todo: rework to new input system

            // chassis
            var steer = Input.GetAxisRaw("Horizontal");
            var speed = Input.GetAxisRaw("Vertical");
            _mechController.SetControls(speed, steer);

            // upper part
            var cabinLeft = Input.GetKey(KeyCode.Q);
            var cabinRight = Input.GetKey(KeyCode.E);
            var cabinRotationValue = cabinLeft ? -1f : (cabinRight ? 1f : 0f);
            if (cabinRotationValue != 0f)
                _mechController.SetUpperPartRotation(cabinRotationValue, deltaTime);

            // main weapons target
            var currentTargetData = _aimWorker.CurrentTargetData;
            var pos = currentTargetData.Position;
            _mechController.SetMainWeaponsTarget(pos);
            _mechController.SetEyesTarget(pos);

            // main weapons shot
            if (Input.GetMouseButtonDown(0))
                _mechController.FireMainLeftWeapon();

            if (Input.GetMouseButtonDown(1))
                _mechController.FireMainRightWeapon();

            // eyes shot
            _eyesActiveProperty.Value = Input.GetKey(KeyCode.Space);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
            _mechController?.Dispose();
            _eyesActiveProperty.Dispose();
        }

        private void OnPlayerViewLoaded(LocalPlayerVehicleAssignedFlag flag)
        {
            _mechController?.Dispose();
            _mechController = _workersFactory.CreateWorker<MechControlsWorker>();
            _mechController.Start(flag.VehicleEntity);

            _eyesActiveProperty
                .Subscribe(isPressed => _mechController.SwitchEyeFiring(isPressed))
                .AddTo(_compositeDisposable);

            var controllerFlag = _flags.AddTemporalFlag<LocalPlayerMechControllerSetFlag>(new(_mechController));
            _mechController.AddToLifetime(controllerFlag);
        }
    }
}