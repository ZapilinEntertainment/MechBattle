using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Units.Squads
{
    public class SquadDecreeApplier
    {
        private readonly INavigationMap _map;

        private readonly Stash<MoveTargetComponent> _moveTargets;
        private readonly Stash<AttackTargetComponent> _attackTargets;
        private readonly Stash<StateComponent> _states;
        private readonly Stash<SquadUpdatedTag> _squadUpdatedTags;


        [Inject]
        public SquadDecreeApplier(World world, INavigationMap map)
        {
            _map = map;

            _moveTargets = world.GetStash<MoveTargetComponent>();
            _attackTargets = world.GetStash<AttackTargetComponent>();
            _states = world.GetStash<StateComponent>();
            _squadUpdatedTags = world.GetStash<SquadUpdatedTag>();
        }

        public void SetSquadMoveDecree(Entity squad, IntTriangularPos tripos)
        {
            _moveTargets.Set(squad, new(tripos, _map));
            _states.Get(squad).NextState = StateKey.Move;
            _squadUpdatedTags.Set(squad);
        }

        public void SetSquadAttackDecree(Entity squad, Entity attackTarget)
        {
            _attackTargets.Set(squad, new() { Entity = attackTarget});
            _states.Get(squad).NextState = StateKey.Attack;
            _squadUpdatedTags.Set(squad);
        }
    
    }
}
