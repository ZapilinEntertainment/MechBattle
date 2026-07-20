using Scellecs.Morpeh;
using UnityEngine;
using Unity.Mathematics;
using VContainer;

namespace ZE.MechBattle.Ecs.States
{
    public class DefaultAttackState : StateHandler
    {
        private readonly Stash<AttackOpportunintyComponent> _attackOpportuninties;
        private readonly Stash<AttackTargetComponent> _attackTargets;
        private readonly Stash<PositionComponent> _positionComponents;
        private readonly MoveTargetApplier _moveTargetApplier;

        [Inject]
        public DefaultAttackState(World world, MoveTargetApplier moveTargetApplier) 
        { 
            _moveTargetApplier = moveTargetApplier;
            _attackTargets = world.GetStash<AttackTargetComponent>();
            _positionComponents = world.GetStash<PositionComponent>();
            _attackOpportuninties = world.GetStash<AttackOpportunintyComponent>();
        }

        public override void Enter(Entity entity) { }

        public override void Exit(Entity entity) 
        { 
            _attackTargets.Remove(entity);
            _attackOpportuninties.Remove(entity);
        }

        public override StateKey Update(Entity entity, float dt)
        {
            // todo: add fire line check via triangle map

            var attackTarget = _attackTargets.Get(entity, out var attackTargetExists);
            if (!attackTargetExists)
                return StateKey.Idle;

            var attackOpportunityComponent = _attackOpportuninties.Get(entity, out var haveAttackOpportunity);
            if (!haveAttackOpportunity || attackOpportunityComponent.Value == 0f)
            {
                var targetPos = _positionComponents.Get(attackTarget.Entity).Value;
                _moveTargetApplier.SetMoveTarget(entity, targetPos);
                return StateKey.Move;
            }
                

            //todo: add minimum distance check (need to add logic into move state)
            return StateKey.Attack;
        }
    }
}
