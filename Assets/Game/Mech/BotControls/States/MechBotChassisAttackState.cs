using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.MechMovement;

namespace ZE.MechBattle
{
    public class MechBotChassisAttackState : MechBotChassisMoveState
    {
        private readonly World _world;
        private readonly MoveTargetApplier _moveTargetApplier;

        private readonly Stash<AttackTargetComponent> _attackTargets;
        private readonly Stash<ParentEntityComponent> _parents;
        private readonly Stash<AttackRangeComponent> _attackRanges;

        [Inject]
        public MechBotChassisAttackState(World world,
            TransformAspectHandler transformAspectHandler,
            MechMovementHandler mechMovementHandler,
            MoveTargetApplier moveTargetApplier) : base(world, transformAspectHandler, mechMovementHandler)
        {
            _world = world;
            _moveTargetApplier = moveTargetApplier;

            _attackTargets = _world.GetStash<AttackTargetComponent>();
            _parents = _world.GetStash<ParentEntityComponent>();
            _attackRanges = _world.GetStash<AttackRangeComponent>();
        }

        public override void Exit(Entity entity)
        {
            base.Exit(entity);
            _attackTargets.Remove(entity);
        }

        public override StateKey Update(Entity chassisEntity, float dt)
        {
            var mechEntity = _parents.Get(chassisEntity).Value;
            var targetComponent = _attackTargets.Get(mechEntity, out var targetExists);
            if (!targetExists)
                return StateKey.Idle;

            var attackRange = _attackRanges.Get(mechEntity, out var exists).Value;

            var targetPosition = _transformAspectHandler.GetPosition(targetComponent.Entity);
            var currentPosition = _transformAspectHandler.GetPosition(chassisEntity);
            var dir = math.normalizesafe(currentPosition - targetPosition);
            var worldPos = targetPosition + attackRange * MechConstants.ATTACK_RANGE_CF * dir;
            _moveTargetApplier.SetMoveTarget(chassisEntity, worldPos);
            return StateKey.Attack;
        }
    }
}
