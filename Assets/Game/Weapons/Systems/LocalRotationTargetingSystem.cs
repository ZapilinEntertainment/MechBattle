using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class LocalRotationTargetingSystem : PausableSystem
    {
        private Filter _unlimitedRotationsFilter;
        private Filter _limitedRotationsFilter;
        private Stash<LocalTargetRotationComponent> _localTargetRotations;
        private Stash<RotationSpeedComponent> _rotationSpeeds;
        private Stash<LocalRotationLimitComponent> _localRotationLimits;
        private readonly TransformAspectHandler _transformAspectHandler;

        public LocalRotationTargetingSystem(SceneFlagsManager flags, TransformAspectHandler transformAspectHandler) : base(flags)
        {
            _transformAspectHandler = transformAspectHandler;
        }

        public override void OnAwake()
        {
            _unlimitedRotationsFilter = World.Filter
                .With<LocalRotationComponent>()
                .With<LocalTargetRotationComponent>()
                .Without<LocalRotationLimitComponent>()
                .Build();

            _limitedRotationsFilter = World.Filter
                .With<LocalRotationComponent>()
                .With<LocalTargetRotationComponent>()
                .With<LocalRotationLimitComponent>()
                .Build();

            _localTargetRotations = World.GetStash<LocalTargetRotationComponent>();
            _rotationSpeeds = World.GetStash<RotationSpeedComponent>();
            _localRotationLimits = World.GetStash<LocalRotationLimitComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;

            foreach (var entity in _unlimitedRotationsFilter)
            {
                var targetRotation = _localTargetRotations.Get(entity).Value;
                var rotationSpeed = _rotationSpeeds.Get(entity).RadianValue;
                _transformAspectHandler.RotateLocal(entity, targetRotation, rotationSpeed * deltaTime);
            }

            foreach (var entity in _limitedRotationsFilter)
            {
                var targetRotation = _localTargetRotations.Get(entity).Value;
                var rotationSpeed = _rotationSpeeds.Get(entity).RadianValue;
                var limits = _localRotationLimits.Get(entity).DotLimits;

                _transformAspectHandler.RotateLocalWithLimits(entity, targetRotation, rotationSpeed * deltaTime, limits);
            }
        }
    }
}