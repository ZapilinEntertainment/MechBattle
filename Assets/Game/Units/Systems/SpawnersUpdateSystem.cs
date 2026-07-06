using Scellecs.Morpeh;
using VContainer;
using Unity.Mathematics;
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
        private Stash<SpawnOperationsLeftComponent> _spawnOperationsLeftComponents;

        private readonly UnitSpawnRequestsFactory _requestsFactory;
        private readonly MultipointSpawnHandler _multipointSpawnHandler;
        private readonly DisposeTagApplier _disposeTagApplier;
        private readonly ISpawnerClearHandler _spawnerClearHandler;

        [Inject]
        public SpawnersUpdateSystem(
            SceneFlagsManager flags, 
            UnitSpawnRequestsFactory requestsFactory, 
            MultipointSpawnHandler multipointSpawnHandler,
            DisposeTagApplier disposeTagApplier,
            ISpawnerClearHandler spawnerClearHandler) : base(flags)
        {
            _requestsFactory = requestsFactory;
            _multipointSpawnHandler = multipointSpawnHandler;            
            _disposeTagApplier = disposeTagApplier;
            _spawnerClearHandler = spawnerClearHandler;
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
            _spawnOperationsLeftComponents = World.GetStash<SpawnOperationsLeftComponent>();
        }

        protected override void IntervalUpdate(Entity entity)
        {
            var spawnComponent = _spawnerComponents.Get(entity);
            var spawnerTripos = _triangularPosComponents.Get(entity).Value;
            var playerKey = _playerAffiliations.Get(entity).PlayerKey;

            ref var spawnLimitComponent = ref _spawnOperationsLeftComponents.Get(entity, out var limitExists);
            var spawnCount = limitExists ? (math.min(spawnLimitComponent.Value, spawnComponent.Count)) : spawnComponent.Count;

            if (spawnCount != 1)
            {
                var protocol = new MultipointSpawnHandler.ExecutionProtocol(entity, spawnComponent, playerKey);
                // note about limits possibility
                protocol.Count = spawnCount;
                _multipointSpawnHandler.Handle(protocol);                
            }
            else
            {
                _requestsFactory.CreateSpawnRequest(spawnComponent.UnitKey, spawnerTripos, playerKey);
            }

            if (limitExists)
            {
                spawnLimitComponent.Value -= spawnCount;
                if (spawnLimitComponent.Value == 0)
                {
                    if (spawnLimitComponent.Strategy == SpawnLimitExhaustedStrategy.Dispose)
                    {
                        _disposeTagApplier.Apply(entity);
                    }                        
                    else
                    {
                        // yes, it will be more correct to make an "SpawnerExhaustedTag" and system for clearing
                        // however it is unclear how many limited spawner will be in game
                        _spawnerClearHandler.ClearSpawnData(entity);
                    }

                }
            }
        }
    }
}