using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Weapons;

namespace ZE.MechBattle
{
    public class MechBotAttackState : MechBotStateBase
    {
        protected override StateKey StateKey => StateKey.Attack;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly MechControlsHandler _controlsHandler;

        private readonly Stash<AttackTargetComponent> _attackTargets;
        private readonly Stash<AttackRangeComponent> _attackRanges;
        private readonly Stash<LocalTargetRotationComponent> _localRotationTargets;
        

        [Inject]
        public MechBotAttackState(
            World world, 
            TransformAspectHandler transformAspectHandler, 
            MechHandler mechHandler,
            MoveTargetApplier moveTargetApplier,
            MechControlsHandler controlsHandler) : base(mechHandler, world, moveTargetApplier)
        {
            _transformAspectHandler = transformAspectHandler;
            _controlsHandler = controlsHandler;

            _attackTargets = world.GetStash<AttackTargetComponent>();
            _attackRanges = world.GetStash<AttackRangeComponent>();
            _localRotationTargets = world.GetStash<LocalTargetRotationComponent>();
        }        

        public override StateKey Update(Entity entity, float dt)
        {
            var attackTargetComponent = _attackTargets.Get(entity, out var haveAttackTarget);
            if (!haveAttackTarget)
                return StateKey.Idle;

            var targetPos = _transformAspectHandler.GetPosition(attackTargetComponent.Entity);
            var mechPos = _transformAspectHandler.GetPosition(entity);
            var attackRange = _attackRanges.Get(entity).Value;

            var rangeCf = math.distancesq(targetPos, mechPos) / (attackRange * attackRange);
            if (rangeCf > 0.8f)
            {
                var normalizedDir = math.normalizesafe(mechPos - targetPos);
                SetMoveTarget(entity, targetPos + attackRange * MechConstants.ATTACK_RANGE_CF * normalizedDir);
                return StateKey.Move;
            }
            else
            {
                var upperPart = _controlsHandler.GetUpperPartEntity(entity);

                _controlsHandler.SetMainWeaponsTarget(entity, upperPart, attackTargetComponent.Entity);

                var localTargetPos = MathExtensions.InverseTransformPoint(targetPos, _transformAspectHandler.GetPoint(entity));
                var normalized = math.normalizesafe(new float3(localTargetPos.x, 0f, localTargetPos.z));

                _localRotationTargets.Set(upperPart, new() { Value = quaternion.LookRotation(normalized, math.up()) });

                //if (math.dot(math.forward(), normalized) < 0.8f)
                //    _controlsHandler.RotateUpperPart(entity, upperPart, dt * (localTargetPos.x > 0f ? 1f : -1f));

                return StateKey.Attack;
            }            
        }

        public override void Exit(Entity entity)
        {
            base.Exit(entity);
            _localRotationTargets.Set(_controlsHandler.GetUpperPartEntity(entity), new() { Value = quaternion.identity });
        }
    }
}
