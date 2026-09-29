using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Units.Squads;

namespace ZE.MechBattle
{
    public class SquadHandler
    {
        private readonly SquadsManager _squadsManager;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly Stash<SquadUpdateRequiredTag> _changedTags;
        private readonly Stash<SquadMemberComponent> _squadMembers;
        private readonly Stash<SquadComponent> _squadComponents;
        private readonly Filter _squadsFilter;

        private readonly SquadMembersIterator _squadMembersIterator;

        [Inject]
        public SquadHandler(World world, SquadsManager squadsManager, TransformAspectHandler transformAspectHandler)
        {
            _squadsManager = squadsManager;
            _transformAspectHandler = transformAspectHandler;

            _changedTags = world.GetStash<SquadUpdateRequiredTag>();
            _squadMembers = world.GetStash<SquadMemberComponent>();
            _squadComponents = world.GetStash<SquadComponent>();

            var membersFilter = world.Filter.With<SquadMemberComponent>().Build();
            _squadMembersIterator = new(membersFilter, _squadMembers, _squadComponents);

            _squadsFilter = world.Filter.With<SquadComponent>().Build();
        }

        public IEnumerable<(Entity memberEntity, int memberIndex)> GetNextSquadMember(Entity squadEntity) => 
            _squadMembersIterator.GetNextSquadMember(squadEntity);

        public IEnumerable<Entity> GetNextSquad()
        {
            foreach (var squadEntity in _squadsFilter)
            {
                yield return squadEntity;
            }
        }

        public float3 GetSquadPosition(Entity squadEntity) => _transformAspectHandler.GetPosition(squadEntity);

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
