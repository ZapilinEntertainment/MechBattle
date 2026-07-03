using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UnitsCreationSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<UnitSpawnRequestComponent> _spawnRequests { get; set;}
        private Stash<PlayerAffiliationComponent> _affiliations { get; set;}

        private readonly DelayApplier _delayApplier;
        private readonly UnitsFactory _unitsFactory;
        private readonly INavigationGridHandler _gridHandler;
        private readonly float _triangleHeight;
        private const float UNSUCCESSFUL_REQUEST_CLEAR_TIME = 5f;

        [Inject]
        public UnitsCreationSystem(
            INavigationGridHandler gridHandler, 
            INavigationMap map,
            DelayApplier delayApplier, 
            UnitsFactory unitsFactory,
            TransformAspectHandler transformAspectHandler)
        {
            _gridHandler = gridHandler;
            _delayApplier = delayApplier;
            _unitsFactory = unitsFactory;

            _triangleHeight = map.TriangleHeight;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<UnitSpawnRequestComponent>().Build();

            _spawnRequests = World.GetStash<UnitSpawnRequestComponent>();
            _affiliations = World.GetStash<PlayerAffiliationComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var requestEntity in _filter)
            {
                var spawnComponent = _spawnRequests.Get(requestEntity);
                var cellPoint = spawnComponent.CellPoint;

                // note: request will not be deleted, it just spawns when cell will be empty or will be cleared in UNSUCCESSFUL_REQUEST_CLEAR_TIME
                if (_gridHandler.IsCellOccupied(cellPoint.Tripos)) 
                {
                    if (!_delayApplier.HasDestructionDelay(requestEntity))
                        _delayApplier.ApplyDestructionDelay(requestEntity, UNSUCCESSFUL_REQUEST_CLEAR_TIME);
                    continue;
                }

                var entity = _unitsFactory.Build(spawnComponent.UnitKey, spawnComponent.CellPoint.ToRigidTransform(_triangleHeight));               
                _affiliations.Add(entity, new(spawnComponent.PlayerKey));

                World.RemoveEntity(requestEntity);
            }
        }

        public void Dispose()
        {

        }
    }
}