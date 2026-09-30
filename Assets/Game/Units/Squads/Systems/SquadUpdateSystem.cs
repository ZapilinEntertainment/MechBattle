using Scellecs.Morpeh;
using System;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SquadUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _changedSquadsFilter;
        private Filter _squadMembersFilter;
        private Stash<SquadUpdateRequiredTag> _updateRequests;
        private Stash<SquadUpdatedTag> _updatedTags;
        private Stash<SquadMemberComponent> _squadMembers;
        private Stash<SquadComponent> _squads;
        private Stash<EntityDisposeTag> _disposeTags;

        private readonly Dictionary<Entity, int> _squadsCount = new();


        public void OnAwake() 
        {
            _changedSquadsFilter = World.Filter
                .With<SquadComponent>()
                .With<SquadUpdateRequiredTag>()
                .Build();

            _squadMembersFilter = World.Filter
                .With<SquadMemberComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _updateRequests = World.GetStash<SquadUpdateRequiredTag>();
            _squadMembers = World.GetStash<SquadMemberComponent>();
            _squads = World.GetStash<SquadComponent>();
            _disposeTags = World.GetStash<EntityDisposeTag>();
            _updatedTags = World.GetStash<SquadUpdatedTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_changedSquadsFilter.IsEmpty())
                return;

            foreach (var entity in _squadMembersFilter)
            {
                var squadEntity = _squadMembers.Get(entity).SquadEntity;
                var membersCount = _squadsCount.GetValueOrDefault(squadEntity);
                _squadsCount[squadEntity] = membersCount + 1;
            }

            foreach (var squadDataKvp in _squadsCount)
            {
                var squadEntity = squadDataKvp.Key;

                if (!_updateRequests.Has(squadEntity))
                    continue;

                var membersCount = squadDataKvp.Value;
                //UnityEngine.Debug.Log($"squad {squadId} : {membersCount} members");
                if (membersCount == 0)
                {
                    _disposeTags.Set(squadEntity);
                }

                ref var squadComponent = ref _squads.Get(squadEntity);
                squadComponent.MembersCount = membersCount;

                ReassignIndices(squadEntity, membersCount);
                _updatedTags.Set(squadEntity);
            }

            _squadsCount.Clear();
            _updateRequests.RemoveAll();
        }

        public void Dispose() { }

        private void ReassignIndices(Entity squadEntity, int membersCount)
        {
            Span<bool> fulfillmentMap = stackalloc bool[membersCount];
            for (var i = 0; i < membersCount; i++)
            {
                fulfillmentMap[i] = false;
            }    
            var nextFreeIndex = 0;

            foreach (var entity in _squadMembersFilter)
            {
                ref var squadMemberComponent = ref _squadMembers.Get(entity);
                if (squadMemberComponent.SquadEntity != squadEntity)
                    continue;

                var index = squadMemberComponent.Index;
                if (index < 0 || index >= membersCount || fulfillmentMap[index])
                {
                    // search for next suitable index
                    for (; nextFreeIndex < membersCount; nextFreeIndex++)
                    {
                        if (!fulfillmentMap[nextFreeIndex])
                            break;
                    }
                }
                else
                {
                    fulfillmentMap[index] = true;
                }

                squadMemberComponent.Index = nextFreeIndex++;
            }
        }
    }
}