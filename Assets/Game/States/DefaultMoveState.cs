using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs.States
{
    public class DefaultMoveState : StateHandler
    {
        private readonly MoveTargetApplier _moveTargetApplier;
        private readonly Stash<MoveTargetComponent> _moveTargets;
        private readonly Stash<AttackTargetComponent> _attackTargets;
        private readonly Stash<TriangularPosComponent> _triangularPosComponents;

        [Inject]
        public DefaultMoveState(World world, MoveTargetApplier moveTargetApplier)
        {
            _moveTargetApplier = moveTargetApplier;

            _moveTargets = world.GetStash<MoveTargetComponent>();
            _attackTargets = world.GetStash<AttackTargetComponent>();
            _triangularPosComponents = world.GetStash<TriangularPosComponent>();
        }

        public override void Enter(Entity entity)
        {
        }

        public override void Exit(Entity entity)
        {
            _moveTargets.Remove(entity);
        }

        public override StateKey Update(Entity entity, float dt)
        {
            var moveTargetComponent = _moveTargets.Get(entity, out var moveTargetExists);
            if (!moveTargetExists)
            {
                //UnityEngine.Debug.Log($"attack target: {attackTargetExists}, move target: {moveTargetExists}");
                return StateKey.Idle;
            }

            // why do target check inside state:
            // entity may run way or going through (different order), but attack some target simulataneously
            var attackTargetComponent = _attackTargets.Get(entity, out var attackTargetExists);
            if (attackTargetExists) 
            { 
                var targetEntity = attackTargetComponent.Entity;
                var targetTripos = _triangularPosComponents.Get(targetEntity).Value;
                if (targetTripos != moveTargetComponent.TriangularPos)
                {
                    _moveTargetApplier.SetMoveTarget(entity, targetEntity);
                }
            }


            return StateKey.Move;
        }
    }
}
