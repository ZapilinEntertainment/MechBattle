using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SquadLossesUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<SquadMemberComponent> _squadMembers;
        private Stash<SquadUpdateRequiredTag> _changedTags;
        private readonly SquadsManager _squadsManager;

        [Inject]
        public SquadLossesUpdateSystem(SquadsManager squadsManager)
        {
            _squadsManager = squadsManager;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<SquadMemberComponent>()
                .With<EntityDisposeTag>()
                .Build();

            _squadMembers = World.GetStash<SquadMemberComponent>();
            _changedTags = World.GetStash<SquadUpdateRequiredTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var squadId = _squadMembers.Get(entity).SquadId;
                if (_squadsManager.TryGetSquad(squadId, out var squadEntity))
                    _changedTags.Set(squadEntity);
            }
        }

        public void Dispose() { }
    }
}