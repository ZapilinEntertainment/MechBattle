using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectileGravityAffectionSystem : PausableSystem 
    {
        private Filter _filter;
        private Stash<MoveSpeedComponent> _moveSpeed;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly float3 _gravity;

        public ProjectileGravityAffectionSystem(SceneFlagsManager flags, TransformAspectHandler transformAspectHandler) : base(flags)
        {
            _transformAspectHandler = transformAspectHandler;
            _gravity = Physics.gravity;
        }

        public override void OnAwake()
        {
            _filter = World.Filter.With<GravityAffectedProjectileTag>().Build();
            _moveSpeed = World.GetStash<MoveSpeedComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused || _filter.IsEmpty())
                return;

            var gravityVector = deltaTime * _gravity;

            foreach (var entity in _filter)
            {
                ref var moveComponent = ref _moveSpeed.Get(entity);
                var newMoveVector = moveComponent.Value * _transformAspectHandler.GetForward(entity) + gravityVector;
                var newLength = math.length(newMoveVector);
                _transformAspectHandler.SetRotation(entity, quaternion.LookRotationSafe(newMoveVector / newLength, math.up()));
                moveComponent.Value = newLength;
            }
        }
    }
}