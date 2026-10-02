using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle
{
    public class SquadAttackState : StateHandler
    {
        protected readonly Stash<SquadUpdatedTag> _squadUpdatedTags;
        protected readonly Stash<AttackTargetComponent> _attackTargetComponents;
        protected readonly Stash<CompositeTargetSpecifiedTag> _compositeTargetSpecified;
        protected readonly Stash<CompositeTargetComponent> _compositeTargets;
        protected readonly SquadHandler _squadHandler;
        protected readonly World _world;

        [Inject]
        public SquadAttackState(World world, SquadHandler squadHandler)
        {
            _world = world;
            _squadHandler = squadHandler;

            _squadUpdatedTags = _world.GetStash<SquadUpdatedTag>();
            _attackTargetComponents = _world.GetStash<AttackTargetComponent>();
            _compositeTargetSpecified = _world.GetStash<CompositeTargetSpecifiedTag>();
            _compositeTargets = _world.GetStash<CompositeTargetComponent>();
        }

        public override void Enter(Entity entity)
        {
            
        }

        public override void Exit(Entity entity)
        {
            _attackTargetComponents.Remove(entity);
            foreach (var (memberEntity, _) in _squadHandler.GetNextSquadMember(entity))
            {
                _attackTargetComponents.Remove(memberEntity);
            }
        }

        public override StateKey Update(Entity entity, float dt)
        {
            if (!_squadUpdatedTags.Has(entity))
                return StateKey.Attack;

            var targetComponent = _attackTargetComponents.Get(entity, out var targetExists);
            if (!targetExists || _world.IsDisposed(targetComponent.Entity))
                return StateKey.Idle;

            UpdateMembersAttackTarget(entity, targetComponent.Entity);
            return StateKey.Attack;
        }

        protected void UpdateMembersAttackTarget(Entity squadEntity, Entity squadTarget)
        {
            foreach (var (memberEntity, _) in _squadHandler.GetNextSquadMember(squadEntity))
            {
                ref var memberTargetComponent = ref _attackTargetComponents.Get(memberEntity, out var haveTarget);
                // undefined problem where composite targeting receives also specified tag before actual specifying
                // todo: investigate
                if (!haveTarget || (_compositeTargets.Has(memberTargetComponent.Entity) && _compositeTargetSpecified.Has(memberEntity)))
                {
                    _compositeTargetSpecified.Remove(memberEntity);
                    _attackTargetComponents.Set(memberEntity, new() { Entity = squadTarget });
                }
            }
        }
    }
}
