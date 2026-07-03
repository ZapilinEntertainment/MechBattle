using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SpawnersUpdateSystem : IntervalUpdateSystemBase<SpawnIntervalComponent>
    {
        private Stash<SpawnerComponent> _spawnerComponents;
        private Stash<TriangularPosComponent> _triangularPosComponents;
        private Stash<PlayerAffiliationComponent> _playerAffiliations;
        private readonly UnitSpawnRequestsFactory _requestsFactory;
        private readonly MultipointSpawnHandler _multipointSpawnHandler;

        [Inject]
        public SpawnersUpdateSystem(SceneFlagsManager flags, UnitSpawnRequestsFactory requestsFactory, MultipointSpawnHandler multipointSpawnHandler) : base(flags)
        {
            _requestsFactory = requestsFactory;
            _multipointSpawnHandler = multipointSpawnHandler;
        }

        protected override FilterBuilder PrepareFilter() =>
            base.PrepareFilter()
            .Without<InitialDelayComponent>();

        public override void OnAwake()
        {
            base.OnAwake();

            _spawnerComponents = World.GetStash<SpawnerComponent>();
            _triangularPosComponents = World.GetStash<TriangularPosComponent>();
            _playerAffiliations = World.GetStash<PlayerAffiliationComponent>();
        }

        protected override void IntervalUpdate(Entity entity)
        {
            var spawnComponent = _spawnerComponents.Get(entity);
            var spawnerTripos = _triangularPosComponents.Get(entity).Value;
            var playerKey = _playerAffiliations.Get(entity).PlayerKey;

            if (spawnComponent.Count == 1)
            {
                _requestsFactory.CreateSpawnRequest(spawnComponent.UnitKey, spawnerTripos, playerKey);
            }
            else
            {
                _multipointSpawnHandler.Handle(entity, spawnComponent, playerKey);
            }
        }
    }
}