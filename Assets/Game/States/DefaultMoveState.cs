using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs.States
{
    public class DefaultMoveState : StateHandler
    {
        private readonly Stash<MoveTargetComponent> _moveTargets;
        private readonly Stash<MoveSpeedComponent> _speed;
        private readonly Stash<RotationSpeedComponent> _angSpeed;
        private readonly TransformAspectHandler _transformAspectHandler;

        [Inject]
        public DefaultMoveState(World world)
        {
            _moveTargets = world.GetStash<MoveTargetComponent>();
            _speed = world.GetStash<MoveSpeedComponent>();
            _angSpeed = world.GetStash<RotationSpeedComponent>();
            _transformAspectHandler = new(world);
        }

        public override void Enter(Entity entity)
        {
        }

        public override void Exit(Entity entity)
        {
        }

        public override StateKey Update(Entity entity, float dt)
        {
            var point = _transformAspectHandler.GetPoint(entity);
            var targetPos = _moveTargets.Get(entity).Value;

            var fwd = math.mul(point.rot, math.forward());
            var dir = targetPos - point.pos;
            var dirLength = math.length(dir);
            var normalizedDir = dir / dirLength;
            var dot = math.dot(normalizedDir, fwd);
            if (math.abs(dot - 1f) > math.EPSILON)
            {
                var targetRot = quaternion.LookRotation(normalizedDir, math.up());
                var angSpeed = _angSpeed.Get(entity).Value;
                _transformAspectHandler.SetRotation(entity, MathExtensions.RotateTowards(point.rot, targetRot, dt * angSpeed));
                return StateKey.Move;
            }
            else
            {               
                var step = _speed.Get(entity).Value * dt;
                if (step >= dirLength)
                {
                    _transformAspectHandler.SetPosition(entity, targetPos);
                    _moveTargets.Remove(entity);
                    return StateKey.Idle;
                }
                else
                {
                    _transformAspectHandler.SetPosition(entity, point.pos + step * normalizedDir );
                    return StateKey.Move;
                }
            }
        }
    }
}
