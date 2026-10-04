using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UnitsCreationSystem : RequestHandleSystemBase<UnitSpawnRequestComponent> 
    {
        private readonly DelayApplier _delayApplier;
        private readonly NavigationGridHandler _gridHandler;
        private readonly SquadHandler _squadHandler;
        private readonly AffinityHandler _affinityHandler;
        private readonly UnitsFactory _unitsFactory;

        private readonly float _triangleHeight;
        private const float UNSUCCESSFUL_REQUEST_CLEAR_TIME = 5f;

        [Inject]
        public UnitsCreationSystem(
            NavigationGridHandler gridHandler, 
            INavigationMap map,
            DelayApplier delayApplier, 
            UnitsFactory unitsFactory,
            TransformAspectHandler transformAspectHandler,
            SquadHandler squadHandler,
            AffinityHandler affinityHandler)
        {
            _gridHandler = gridHandler;
            _delayApplier = delayApplier;
            _squadHandler = squadHandler;
            _affinityHandler = affinityHandler;
            _unitsFactory = unitsFactory;

            _triangleHeight = map.TriangleHeight;
        }

        protected override bool TryHandleRequest(Entity requestEntity)
        {
            var spawnRequest = GetRequestComponent(requestEntity);
            var cellPoint = spawnRequest.CellPoint;

            // note: request will not be deleted, it just spawns when cell will be empty or will be cleared in UNSUCCESSFUL_REQUEST_CLEAR_TIME
            if (_gridHandler.IsCellOccupied(cellPoint.Tripos) || _gridHandler.IsCellObstructed(cellPoint.Tripos))
                return false;

            var unitEntity = _unitsFactory.Build(spawnRequest.UnitKey, spawnRequest.CellPoint.ToRigidTransform(_triangleHeight));
            _affinityHandler.SetEntityPlayerAffinity(unitEntity, spawnRequest.PlayerKey);
            _affinityHandler.AddFriendlyFireProtection(unitEntity, spawnRequest.PlayerKey);

            if (!World.IsDisposed(spawnRequest.SquadEntity))
                _squadHandler.AssignEntityToSquad(unitEntity, spawnRequest.SquadEntity);               

            return true;
        }

        protected override void OnRequestDenied(Entity requestEntity)
        {
            if (!_delayApplier.HasDestructionDelay(requestEntity))
                _delayApplier.ApplyDestructionDelay(requestEntity, UNSUCCESSFUL_REQUEST_CLEAR_TIME);
        }
    }
}