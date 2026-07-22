using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UnitsCreationSystem : EntityCreationSystemBase<UnitSpawnRequestComponent, UnitsFactory> 
    {
        private Stash<PlayerAffiliationComponent> _affiliations { get; set;}

        private readonly DelayApplier _delayApplier;
        private readonly INavigationGridHandler _gridHandler;
        private readonly float _triangleHeight;
        private const float UNSUCCESSFUL_REQUEST_CLEAR_TIME = 5f;

        [Inject]
        public UnitsCreationSystem(
            INavigationGridHandler gridHandler, 
            INavigationMap map,
            DelayApplier delayApplier, 
            UnitsFactory unitsFactory,
            TransformAspectHandler transformAspectHandler) : base(unitsFactory)
        {
            _gridHandler = gridHandler;
            _delayApplier = delayApplier;

            _triangleHeight = map.TriangleHeight;
        }

        public override void OnAwake() 
        {
            base.OnAwake();
            _affiliations = World.GetStash<PlayerAffiliationComponent>();
        }

        protected override bool TryExecuteRequest(Entity requestEntity)
        {
            var spawnComponent = RequestsStash.Get(requestEntity);
            var cellPoint = spawnComponent.CellPoint;

            // note: request will not be deleted, it just spawns when cell will be empty or will be cleared in UNSUCCESSFUL_REQUEST_CLEAR_TIME
            if (_gridHandler.IsCellOccupied(cellPoint.Tripos))
            {
                if (!_delayApplier.HasDestructionDelay(requestEntity))
                    _delayApplier.ApplyDestructionDelay(requestEntity, UNSUCCESSFUL_REQUEST_CLEAR_TIME);
                return false;
            }

            var entity = Factory.Build(spawnComponent.UnitKey, spawnComponent.CellPoint.ToRigidTransform(_triangleHeight));
            _affiliations.Add(entity, new(spawnComponent.PlayerKey));
            return true;
        }
    }
}