using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechBotMoveState : MechBotStateBase
    {
        protected override StateKey StateKey => StateKey.Move;

        private readonly Stash<AttackTargetComponent> _attackTargets;
        private readonly Stash<AttackRangeComponent> _attackRange;
        private readonly Stash<MoveTargetComponent> _moveTargets;
        private readonly TransformAspectHandler _transformAspectHandler;

        [Inject]
        public MechBotMoveState(World world, MechHandler mechHandler, MoveTargetApplier moveTargetApplier, TransformAspectHandler transformAspectHandler) : base(mechHandler, world, moveTargetApplier)
        {
            _transformAspectHandler = transformAspectHandler;

            _attackTargets = world.GetStash<AttackTargetComponent>();
            _attackRange = world.GetStash<AttackRangeComponent>();
            _moveTargets = world.GetStash<MoveTargetComponent>();
        }

        

        public override StateKey Update(Entity entity, float dt)
        {
            var attackTargetComponent = _attackTargets.Get(entity, out var haveAttackTarget);
            if (haveAttackTarget)
            {
                var targetPos = _transformAspectHandler.GetPosition(attackTargetComponent.Entity);
                var mechPos = _transformAspectHandler.GetPosition(entity);
                var attackRange = _attackRange.Get(entity).Value;
                SetMoveTarget(entity, targetPos);
                if (math.distancesq(targetPos, mechPos) < attackRange * attackRange)
                {
                    return StateKey.Attack;
                }
                else
                {
                    return StateKey;
                }
            }
            else
            {
                var chassisEntity = _mechHandler.GetChassisEntity(entity);
                if (_moveTargets.Has(chassisEntity))
                {
                    var chassisState = _stateComponents.Get(entity).CurrentState;
                    if (chassisState == StateKey.Idle) // reached target point
                        return StateKey.Idle;
                    else
                        return StateKey;
                }
                else
                {
                    SyncComponentsCommand.Execute<MoveTargetComponent>(chassisEntity, entity, _moveTargets);
                    return StateKey;
                }
               
            }
        }
    }
}
