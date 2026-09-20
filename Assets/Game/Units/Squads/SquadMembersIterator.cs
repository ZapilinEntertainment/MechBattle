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

        public SquadMembersIterator(World world)
        {            
            _squadMembers = world.GetStash<SquadMemberComponent>();
            _squadComponents = world.GetStash<SquadComponent>();
            _membersFilter = world.Filter.With<SquadMemberComponent>().Without<EntityDisposeTag>().Build();
        }

        public IEnumerable<(Entity memberEntity, int memberIndex)> GetNextSquadMember(Entity squadEntity)
        {
#if UNITY_EDITOR
            if (_membersFilter.IsEmpty())
            {
                //UnityEngine.Debug.LogWarning("members filter is empty, this is not expected");
                yield break;
            }
#endif

            var squadId = _squadComponents.Get(squadEntity).Id;
            foreach (var entity in _membersFilter)
            {
                var squadMemberComponent = _squadMembers.Get(entity);
                if (squadMemberComponent.SquadId != squadId)
                    continue;

                yield return (entity, squadMemberComponent.Index);
            }
        }
    }
}
