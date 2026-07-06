using VContainer;
using Scellecs.Morpeh;
namespace ZE.MechBattle.Ecs.States
{
    public class DefaultIdleState : StateHandler
    {
        private readonly MoveTargetApplier _moveTargetApplier;
        private readonly Stash<AttackTargetComponent> _attackTargets;
        private readonly Stash<MoveTargetComponent> _moveTargets;

        [Inject]
        public DefaultIdleState(World world, MoveTargetApplier moveTargetApplier)
        {
            _moveTargetApplier = moveTargetApplier;

            _attackTargets = world.GetStash<AttackTargetComponent>();
            _moveTargets = world.GetStash<MoveTargetComponent>();
        }

        public override void Enter(Entity entity) { }

        public override void Exit(Entity entity) { }

        public override StateKey Update(Entity entity, float dt)
        {
            if (_moveTargets.Has(entity))
            {
                UnityEngine.Debug.Log($"entity {entity.Id} move target is {_moveTargets.Get(entity).TriangularPos}");
                return StateKey.Move;
            }
                

            var attackTargetComponent = _attackTargets.Get(entity, out var hasAttackTarget);
            if (hasAttackTarget)
            {
                var attackTargetEntity = attackTargetComponent.Entity;
                _moveTargetApplier.SetMoveTarget(entity, attackTargetEntity);      
            }
            return StateKey.Idle;
        }
    }
}
