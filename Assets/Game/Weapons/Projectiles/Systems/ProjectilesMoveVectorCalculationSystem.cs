using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectilesMoveVectorCalculationSystem : PausableSystem
    {
        private Filter _gravityAffectedFilter;
        private Filter _noGravityAffectionFilter;
        private Stash<GravityAccelerationValueComponent> _gravityAcceleration;
        private Stash<MoveSpeedComponent> _moveSpeed;
        private Stash<ProjectileMoveVectorComponent> _moveVectors;
        private readonly TransformAspectHandler _transformAspectHandler;

        public ProjectilesMoveVectorCalculationSystem(SceneFlagsManager flags, TransformAspectHandler transformAspectHandler) : base(flags)
        {
            _transformAspectHandler = transformAspectHandler;
        }

        public override void OnAwake()
        {
            _gravityAffectedFilter = World.Filter
                .With<ProjectileComponent>()
                .With<GravityAccelerationValueComponent>()
                .With<MoveSpeedComponent>()
                .Build();

            _noGravityAffectionFilter = World.Filter
                .With<ProjectileComponent>()
                .Without<GravityAccelerationValueComponent>()
                .With<MoveSpeedComponent>()
                .Build();

            _gravityAcceleration = World.GetStash<GravityAccelerationValueComponent>();
            _moveSpeed = World.GetStash<MoveSpeedComponent>();
            _moveVectors = World.GetStash<ProjectileMoveVectorComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused) 
                return;

            foreach (var entity in _gravityAffectedFilter)
            {
                var moveVector = _moveSpeed.Get(entity).Value * deltaTime * _transformAspectHandler.GetForward(entity);
                var gravityVector = _gravityAcceleration.Get(entity).CurrentValue * deltaTime * math.down();

                var dir = moveVector + gravityVector;
                var length = math.length(dir);
                _moveVectors.Set(entity, new() { NormalizedDirection = dir / length, Magnitude = length});
            }

            foreach (var entity in _noGravityAffectionFilter)
            {
                var dir = _moveSpeed.Get(entity).Value * deltaTime * _transformAspectHandler.GetForward(entity);
                var length = math.length(dir);
                _moveVectors.Set(entity, new() { NormalizedDirection = dir / length, Magnitude = length });
            }
        }
    }
}