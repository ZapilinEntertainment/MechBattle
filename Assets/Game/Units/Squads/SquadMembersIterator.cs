using Scellecs.Morpeh;
using System.Collections.Generic;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Units.Squads
{
    public class SquadMembersIterator
    {
        private readonly Filter _membersFilter;
        private readonly Stash<SquadMemberComponent> _squadMembers;
        private readonly Stash<SquadComponent> _squadComponents;

        public SquadMembersIterator(Filter membersFilter, Stash<SquadMemberComponent> membersStash, Stash<SquadComponent> componentsStash)
        {
            _squadMembers = membersStash;
            _squadComponents = componentsStash;
            _membersFilter = membersFilter;
        }

        public IEnumerable<(Entity memberEntity, int memberIndex)> GetNextSquadMember(Entity squadEntity)
        {
            if (_membersFilter.IsEmpty())
            {
                //UnityEngine.Debug.LogWarning("members filter is empty, this is not expected");
                yield break;
            }

            foreach (var entity in _membersFilter)
            {
                var squadMemberComponent = _squadMembers.Get(entity);
                if (squadMemberComponent.SquadEntity != squadEntity)
                    continue;

                yield return (entity, squadMemberComponent.Index);
            }
        }
    }
}
