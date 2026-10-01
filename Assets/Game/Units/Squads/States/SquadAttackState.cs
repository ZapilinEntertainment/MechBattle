using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle
{
    public class SquadAttackState : StateHandler
    {
        private readonly Stash<SquadUpdatedTag> _squadUpdatedTags;
        private readonly Stash<AttackTargetComponent> _attackTargetComponents;
        private readonly Stash<CompositeTargetSpecifiedTag> _compositeTargetSpecified;
        private readonly Stash<CompositeTargetComponent> _compositeTargets;
        private readonly SquadHandler _squadHandler;
        private readonly World _world;

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

            var squadTarget = targetComponent.Entity;
            foreach (var (memberEntity, _) in _squadHandler.GetNextSquadMember(entity))
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

            return StateKey.Attack;
        }
    }
}
