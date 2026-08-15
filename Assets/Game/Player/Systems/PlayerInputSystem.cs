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
        private Entity _vehicleEntity;
        private Entity _upperPartEntity;
        private Stash<MechInputComponent> _input;
        private Stash<MechComponent> _mechComponents;
        private Stash<RotationSpeedComponent> _rotationSpeed;
        private Stash<WeaponTargetPositionComponent> _weaponTargetPositions;

        [Inject]
        public PlayerInputSystem(
            SceneFlagsManager flags, 
            TransformAspectHandler transformAspectHandler,
            CursorAimTrackingWorker aimWorker)
        {
            _compositeDisposable = new();
            flags
                .Subscribe<LocalPlayerViewInstancedFlag>(OnPlayerViewLoaded)
                .AddTo(_compositeDisposable);
            flags
                .Subscribe<LocalPlayerViewInstancedFlag>(flagActive => _playerVehiclePresented = flagActive)
                .AddTo(_compositeDisposable);

            _transformAspectHandler = transformAspectHandler;
            _aimWorker = aimWorker;
        }

        public void OnAwake() 
        {
            _input = World.GetStash<MechInputComponent>();
            _mechComponents = World.GetStash<MechComponent>();
            _rotationSpeed = World.GetStash<RotationSpeedComponent>();
            _weaponTargetPositions = World.GetStash<WeaponTargetPositionComponent>();

            _aimWorker.Start();
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_playerVehiclePresented)
                return;

            // chassis
            var steer = Input.GetAxisRaw("Horizontal");
            var speed = Input.GetAxisRaw("Vertical");
            _input.Set(_vehicleEntity, new() { SpeedValue = speed, SteerValue = steer });

            // upper part
            var cabinLeft = Input.GetKey(KeyCode.Q);
            var cabinRight = Input.GetKey(KeyCode.E);
            var cabinRotationValue = cabinLeft ? -1f : (cabinRight ? 1f : 0f);
            if (cabinRotationValue != 0f)
            {
                var rotationSpeed = _rotationSpeed.Get(_upperPartEntity).RadianValue;
                var rotationStep = quaternion.AxisAngle(math.up(), deltaTime * cabinRotationValue * rotationSpeed);
                _transformAspectHandler.RotateLocal(_upperPartEntity, rotationStep);
            }

            // weapons
            var currentTargetData = _aimWorker.CurrentTargetData;
            if (currentTargetData.IsDefined)
            {
                var pos = currentTargetData.Position;
                _weaponTargetPositions.Set(_upperPartEntity, new() { Value = pos});
            }
               
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        private void OnPlayerViewLoaded(LocalPlayerViewInstancedFlag flag)
        {
            _vehicleEntity = flag.VehicleEntity;
            _upperPartEntity = _mechComponents.Get(_vehicleEntity).UpperPartEntity;
        }
    }
}