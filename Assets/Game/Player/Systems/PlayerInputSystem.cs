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
        private readonly EntityViewHandler _viewHandler;
        private readonly TransformAspectHandler _transformAspectHandler;

        private bool _playerVehiclePresented = false;
        private Entity _vehicleEntity;
        private Entity _upperPartEntity;
        private Stash<MechInputComponent> _input;
        private Stash<MechComponent> _mechComponents;
        private Stash<RotationSpeedComponent> _rotationSpeed;

        [Inject]
        public PlayerInputSystem(SceneFlagsManager flags, EntityViewHandler viewHandler, TransformAspectHandler transformAspectHandler)
        {
            _compositeDisposable = new();
            flags
                .Subscribe<LocalPlayerViewInstancedFlag>(OnPlayerViewLoaded)
                .AddTo(_compositeDisposable);
            flags
                .Subscribe<LocalPlayerViewInstancedFlag>(flagActive => _playerVehiclePresented = flagActive)
                .AddTo(_compositeDisposable);

            _viewHandler = viewHandler;
            _transformAspectHandler = transformAspectHandler;
        }

        public void OnAwake() 
        {
            _input = World.GetStash<MechInputComponent>();
            _mechComponents = World.GetStash<MechComponent>();
            _rotationSpeed = World.GetStash<RotationSpeedComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_playerVehiclePresented)
                return;

            var steer = Input.GetAxisRaw("Horizontal");
            var speed = Input.GetAxisRaw("Vertical");
            _input.Set(_vehicleEntity, new() { SpeedValue = speed, SteerValue = steer });

            var cabinLeft = Input.GetKey(KeyCode.Q);
            var cabinRight = Input.GetKey(KeyCode.E);
            var cabinRotationValue = cabinLeft ? -1f : (cabinRight ? 1f : 0f);
            if (cabinRotationValue != 0f)
            {
                var rotationSpeed = _rotationSpeed.Get(_upperPartEntity).RadianValue;
                var rotationStep = quaternion.AxisAngle(math.up(), deltaTime * cabinRotationValue * rotationSpeed);
                _transformAspectHandler.RotateLocal(_upperPartEntity, rotationStep);
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