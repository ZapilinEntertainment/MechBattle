using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class SquadHandler
    {
        private readonly INavigationMap _map;
        private readonly SquadsManager _squadsManager;
        private readonly Stash<SquadUpdateRequiredTag> _changedTags;
        private readonly Stash<SquadMemberComponent> _squadMembers;
        private readonly Stash<SquadComponent> _squadComponents;

        [Inject]
        public SquadHandler(World world, INavigationMap map, SquadsManager squadsManager)
        {
            _map = map;
            _squadsManager = squadsManager;

            _changedTags = world.GetStash<SquadUpdateRequiredTag>();
            _squadMembers = world.GetStash<SquadMemberComponent>();
            _squadComponents = world.GetStash<SquadComponent>();
        } 

        public void AssignEntityToSquad(Entity entity, int squadId)
        {
            if (!_squadsManager.TryGetSquad(squadId, out var squadEntity))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning("cannot find squad " + squadId.ToString());
#endif
                return;
            }
            AssignEntityToSquad(entity, squadEntity, squadId);
        }

        private void AssignEntityToSquad(Entity entity, Entity squadEntity, int squadId)
        {
            ref var squadComponent = ref _squadComponents.Get(squadEntity);
            _squadMembers.Set(entity, new(squadId, squadComponent.MembersCount));
            squadComponent.MembersCount += 1;
            _changedTags.Set(squadEntity);
        }

    }
}
