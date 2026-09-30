using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    // will set members target if they are losing it (on attack state ends for example)
    public sealed class SquadMembersAttackTargetSyncSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _squadMembersFilter;
        private Stash<AttackTargetComponent> _attackTargets;
        private Stash<SquadMemberComponent> _squadMemberComponent;
        private readonly SquadHandler _squadHandler;

        [Inject]
        public SquadMembersAttackTargetSyncSystem(SquadHandler squadHandler)
        {
            _squadHandler = squadHandler;
        }

        public void OnAwake() 
        {
            _squadMembersFilter = World.Filter
                .With<SquadMemberComponent>()
                .With<SyncAttackTargetsTag>()
                .Without<AttackTargetComponent>()
                .Build();

            _attackTargets = World.GetStash<AttackTargetComponent>();
            _squadMemberComponent = World.GetStash<SquadMemberComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var unitEntity in _squadMembersFilter)
            {
                var squadEntity = _squadMemberComponent.Get(unitEntity).SquadEntity;
                _attackTargets.Set(unitEntity, _attackTargets.Get(squadEntity));
            }
        }

        public void Dispose() { }
    }
}