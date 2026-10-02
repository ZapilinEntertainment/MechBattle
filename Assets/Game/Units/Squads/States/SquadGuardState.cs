using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class SquadGuardState : SquadAttackState
    {
        private readonly Stash<GuardPositionComponent> _guardPositions;
        private readonly Stash<GuardRadiusComponent> _guardRadiuses;
        private readonly Stash<HexCoordComponent> _hexCoords;
        private readonly MoveTargetApplier _moveTargetApplier;
                
        public SquadGuardState(World world, SquadHandler squadHandler, MoveTargetApplier moveTargetApplier) : base(world, squadHandler)
        {
            _moveTargetApplier = moveTargetApplier;
            _guardPositions = world.GetStash<GuardPositionComponent>();
            _guardRadiuses = world.GetStash<GuardRadiusComponent>();
            _hexCoords = world.GetStash<HexCoordComponent>();
        }

        public override void Enter(Entity entity)
        {
            base.Enter(entity);
            _guardPositions.Set(entity, new(_hexCoords.Get(entity).Value));
            UpdateMembersAttackTarget(entity, _attackTargetComponents.Get(entity).Entity);
        }

        public override StateKey Update(Entity entity, float dt)
        {
            var targetComponent = _attackTargetComponents.Get(entity, out var targetExists);
            if (!targetExists || _world.IsDisposed(targetComponent.Entity))
                return StateKey.Idle;

            var targetEntity = targetComponent.Entity;
            var targetHexCoord = _hexCoords.Get(targetEntity).Value;
            var guardHexCoord = _guardPositions.Get(entity).HexCoord;
            var guardRadius = _guardRadiuses.Get(entity).Value;

            if (HexMath.CalculateHexPosDistance(targetHexCoord, guardHexCoord) > guardRadius + 1)
                return StateKey.Idle;

            if (!_squadUpdatedTags.Has(entity))
                return StateKey.Guard;            

            UpdateMembersAttackTarget(entity, targetEntity);
            return StateKey.Guard;
        }

        public override void Exit(Entity entity)
        {
            base.Exit(entity);
            var guardPosition = _guardPositions.Get(entity).HexCoord;
            _moveTargetApplier.SetMoveTarget(entity, guardPosition);

            foreach (var (member, _) in _squadHandler.GetNextSquadMember(entity))
            {
                _attackTargetComponents.Remove(member);
                _moveTargetApplier.StopMovement(member);
            }
               

            _guardPositions.Remove(entity);
            UnityEngine.Debug.Log("exit guard state");
        }
    }
}
