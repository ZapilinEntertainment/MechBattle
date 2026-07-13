using Scellecs.Morpeh;
using UnityEngine;
using Unity.Mathematics;
using VContainer;

namespace ZE.MechBattle.Ecs.States
{
    public class DefaultAttackState : StateHandler
    {
        private readonly Stash<AttackDistanceComponent> _attackDistances;
        private readonly Stash<AttackTargetComponent> _attackTargets;
        private readonly Stash<PositionComponent> _positionComponents;
        private readonly MoveTargetApplier _moveTargetApplier;

        [Inject]
        public DefaultAttackState(World world, MoveTargetApplier moveTargetApplier) 
        { 
            _moveTargetApplier = moveTargetApplier;
            _attackDistances = world.GetStash<AttackDistanceComponent>();
            _attackTargets = world.GetStash<AttackTargetComponent>();
            _positionComponents = world.GetStash<PositionComponent>();
        }

        public override void Enter(Entity entity) { }

        public override void Exit(Entity entity) { }

        public override StateKey Update(Entity entity, float dt)
        {
            // todo: add fire line check via triangle map

            var attackTarget = _attackTargets.Get(entity, out var attackTargetExists);
            if (!attackTargetExists)
                return StateKey.Idle;

            var entityPos = _positionComponents.Get(entity).Value;
            var targetPos = _positionComponents.Get(attackTarget.Entity).Value;
            var attackDistanceComponent = _attackDistances.Get(entity);
            if (math.distancesq(entityPos, targetPos) > attackDistanceComponent.MaximumSq)
            {
                _moveTargetApplier.SetMoveTarget(entity, entityPos);
                return StateKey.Move;
            }
                

            //todo: add minimum distance check (need to add logic into move state)
            return StateKey.Attack;
        }
    }
}
