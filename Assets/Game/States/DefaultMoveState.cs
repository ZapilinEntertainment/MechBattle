using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs.States
{
    public class DefaultMoveState : StateHandler
    {
        protected readonly Stash<MoveTargetComponent> MoveTargets;
        protected readonly Stash<MoveSpeedComponent> Speed;
        protected readonly Stash<RotationSpeedComponent> AngSpeed;
        protected readonly TransformAspectHandler TransformAspectHandler;

        [Inject]
        public DefaultMoveState(World world, TransformAspectHandler transformAspectHandler)
        {
            MoveTargets = world.GetStash<MoveTargetComponent>();
            Speed = world.GetStash<MoveSpeedComponent>();
            AngSpeed = world.GetStash<RotationSpeedComponent>();
            TransformAspectHandler = transformAspectHandler;
        }

        public override void Enter(Entity entity)
        {
        }

        public override void Exit(Entity entity)
        {
        }

        public override StateKey Update(Entity entity, float dt)
        {
            var point = TransformAspectHandler.GetPoint(entity);
            var targetPos = MoveTargets.Get(entity).WorldPos;

            var fwd = math.mul(point.rot, math.forward());
            var dir = targetPos - point.pos;
            var dirLength = math.length(dir);
            var normalizedDir = dir / dirLength;
            var dot = math.dot(normalizedDir, fwd);
            if (math.abs(dot - 1f) > math.EPSILON)
            {
                var targetRot = quaternion.LookRotation(normalizedDir, math.up());
                var angSpeed = AngSpeed.Get(entity).Value;
                TransformAspectHandler.SetRotation(entity, MathExtensions.RotateTowards(point.rot, targetRot, dt * angSpeed));
                return StateKey.Move;
            }
            else
            {               
                var step = Speed.Get(entity).Value * dt;
                if (step >= dirLength)
                {
                    TransformAspectHandler.SetPosition(entity, targetPos);
                    MoveTargets.Remove(entity);
                    return StateKey.Idle;
                }
                else
                {
                    TransformAspectHandler.SetPosition(entity, point.pos + step * normalizedDir );
                    return StateKey.Move;
                }
            }
        }
    }
}
