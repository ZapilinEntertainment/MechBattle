using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs.States
{
    public class DefaultMoveState : StateHandler
    {
        protected readonly TransformAspectHandler TransformAspectHandler;
        private readonly Stash<MoveTargetComponent> _moveTargets;
        private readonly Stash<MoveSpeedComponent> _speed;
        private readonly Stash<RotationSpeedComponent> _angSpeed;        
        private const float MIN_ANGLE_DOT = 1e-6f;

        [Inject]
        public DefaultMoveState(World world)
        {
            _moveTargets = world.GetStash<MoveTargetComponent>();
            _speed = world.GetStash<MoveSpeedComponent>();
            _angSpeed = world.GetStash<RotationSpeedComponent>();
            TransformAspectHandler = new(world);
        }

        public override void Enter(Entity entity)
        {
        }

        public override void Exit(Entity entity)
        {
        }

        public override StateKey Update(Entity entity, float dt)
        {
            var targetPos = _moveTargets.Get(entity).Value;
            if (TryReachPoint(entity, targetPos, dt))
            {
                _moveTargets.Remove(entity);
                return StateKey.Idle;
            }
            return StateKey.Move;
        }

        protected bool TryReachPoint(Entity entity, float3 targetPos, float dt)
        {
            var point = TransformAspectHandler.GetPoint(entity);

            var fwd = math.mul(point.rot, math.forward());
            var dir = targetPos - point.pos;
            var dirLength = math.length(dir);
            var normalizedDir = dir / dirLength;
            var dot = math.dot(normalizedDir, fwd);
            var dotDelta = math.abs(dot - 1f);
            if (dotDelta > MIN_ANGLE_DOT)
            {
                var targetRot = quaternion.LookRotation(normalizedDir, math.up());
                var angSpeed = _angSpeed.Get(entity).Value;
                TransformAspectHandler.SetRotation(entity, MathExtensions.RotateTowards(point.rot, targetRot, dt * angSpeed));

                return false;
            }
            else
            {
                var step = _speed.Get(entity).Value * dt;
                if (step >= dirLength)
                {
                    TransformAspectHandler.SetPosition(entity, targetPos);                    
                    return true;
                }
                else
                {
                    TransformAspectHandler.SetPosition(entity, point.pos + step * normalizedDir);
                    return false;
                }
            }
        }
    }
}
