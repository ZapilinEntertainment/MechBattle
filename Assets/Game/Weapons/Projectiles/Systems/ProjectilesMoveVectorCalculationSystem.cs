using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectilesMoveVectorCalculationSystem : PausableSystem
    {
        private Filter _filter;
        private Stash<MoveSpeedComponent> _moveSpeed;
        private Stash<ProjectileMoveVectorComponent> _moveVectors;
        private readonly TransformAspectHandler _transformAspectHandler;

        public ProjectilesMoveVectorCalculationSystem(SceneFlagsManager flags, TransformAspectHandler transformAspectHandler) : base(flags)
        {
            _transformAspectHandler = transformAspectHandler;
        }

        public override void OnAwake()
        {
            _filter = World.Filter
                .With<ProjectileComponent>()
                .With<MoveSpeedComponent>()
                .Build();

            _moveSpeed = World.GetStash<MoveSpeedComponent>();
            _moveVectors = World.GetStash<ProjectileMoveVectorComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused) 
                return;

            foreach (var entity in _filter)
            {
                var dir = _moveSpeed.Get(entity).Value * deltaTime * _transformAspectHandler.GetForward(entity);
                var length = math.length(dir);
                _moveVectors.Set(entity, new() { NormalizedDirection = dir / length, Magnitude = length });
            }
        }
    }
}