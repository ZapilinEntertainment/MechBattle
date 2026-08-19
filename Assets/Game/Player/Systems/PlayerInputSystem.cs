using R3;
using Scellecs.Morpeh;
using System;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PlayerInputSystem : ISystem 
    {
        public World World { get; set;}
        private readonly CompositeDisposable _compositeDisposable;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly CursorAimTrackingWorker _aimWorker;

        private bool _playerVehiclePresented = false;
        private MechController _mechController;
        

        [Inject]
        public PlayerInputSystem(
            SceneFlagsManager flags, 
            TransformAspectHandler transformAspectHandler,
            ICursorAimTracker aimWorker)
        {
            _compositeDisposable = new();
            flags
                .Subscribe<LocalPlayerViewInstancedFlag>(OnPlayerViewLoaded)
                .AddTo(_compositeDisposable);
            flags
                .Subscribe<LocalPlayerViewInstancedFlag>(flagActive => _playerVehiclePresented = flagActive)
                .AddTo(_compositeDisposable);

            _transformAspectHandler = transformAspectHandler;
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
                _mechController.FireMainWeapon();

            // eyes shot
            if (Input.GetKeyDown(KeyCode.Space))
                _mechController.SwitchEyeFiring();

        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
            _mechController?.Dispose();
        }

        private void OnPlayerViewLoaded(LocalPlayerViewInstancedFlag flag)
        {
            _mechController?.Dispose();
            _mechController = new MechController(World, _transformAspectHandler, flag.VehicleEntity);
        }
    }
}