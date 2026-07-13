using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class LocalRotationTargetingSystem : PausableSystem
    {
        private Filter _filter;
        private Stash<LocalRotationComponent> _localRotations;
        private Stash<LocalTargetRotationComponent> _localTargetRotations;
        private Stash<RotationSpeedComponent> _rotationSpeeds;
        private readonly TransformAspectHandler _transformAspectHandler;

        public LocalRotationTargetingSystem(SceneFlagsManager flags, TransformAspectHandler transformAspectHandler) : base(flags)
        {
            _transformAspectHandler = transformAspectHandler;
        }

        public override void OnAwake()
        {
            _filter = World.Filter
                .With<LocalRotationComponent>()
                .With<LocalTargetRotationComponent>()
                .Build();

            _localRotations = World.GetStash<LocalRotationComponent>();
            _localTargetRotations = World.GetStash<LocalTargetRotationComponent>();
            _rotationSpeeds = World.GetStash<RotationSpeedComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;

            foreach (var entity in _filter)
            {
                var targetRotation = _localTargetRotations.Get(entity).Value;
                var rotationSpeed = _rotationSpeeds.Get(entity).RadianValue;
                if (_transformAspectHandler.RotateLocal(entity, targetRotation, rotationSpeed * deltaTime))
                    _localTargetRotations.Remove(entity);
            }
        }
    }
}