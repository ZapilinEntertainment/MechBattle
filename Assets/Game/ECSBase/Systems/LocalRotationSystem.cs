using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class LocalRotationSystem : PausableSystem
    {
        private Filter _unlimitedRotationsFilter;
        private Filter _limitedRotationsFilter;
        private Stash<LocalTargetRotationComponent> _localTargetRotations;
        private Stash<RotationSpeedComponent> _rotationSpeeds;
        private Stash<LocalRotationLimitComponent> _localRotationLimits;
        private Stash<LocalRotationComponent> _localRotations;
        private readonly TransformAspectHandler _transformAspectHandler;

        public LocalRotationSystem(SceneFlagsManager flags, TransformAspectHandler transformAspectHandler) : base(flags)
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
            _localRotations = World.GetStash<LocalRotationComponent>();
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

            if (_limitedRotationsFilter.IsNotEmpty())
            {
                foreach (var entity in _limitedRotationsFilter)
                {
                    _transformAspectHandler.AddUpdateTag(entity);
                }
                World.Commit();

                var filter = _limitedRotationsFilter.AsNative();
                var job = new LocalLimitedRotationJob()
                {
                    Filter = filter,
                    DeltaTime = deltaTime,
                    Limits = _localRotationLimits.AsNative(),
                    LocalRotations = _localRotations.AsNative(),
                    LocalTargetRotations = _localTargetRotations.AsNative(),
                    RotationSpeeds = _rotationSpeeds.AsNative()
                };
                World.JobHandle = job.Schedule(filter.length, 16);
#if MORPEH_JOB_TRACKING
            UnityEngine.Debug.Log("local limited rotation job");
#endif
            }

        }
    }
}