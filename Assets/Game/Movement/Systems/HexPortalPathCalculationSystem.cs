using System.Buffers;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using Unity.Collections;
using ZE.MechBattle.Navigation;
using ZE.Utils;


namespace ZE.MechBattle.Ecs {

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPortalPathCalculationSystem : PathCalculationSystemBase<HexPortalsPath>
    {
        protected override int MAX_CACHED_STATUSES_COUNT => 64;
        protected override Filter Filter => _filter;
        protected override IProcessManager<PathCalculationProcessToken> ProcessManager => _processesManager;

        protected override IEntityPathValidator<HexPortalsPath> PathValidator => _validator;

        private Filter _filter;

        private Stash<HexPathCalculationRequestTag> _calculationTags;
        private Stash<HexPathProgressionComponent> _progressionComponents;
        private Stash<TriangularPosComponent> _triangularPosComponents;
        private Stash<MoveTargetComponent> _moveTargets;
        private Stash<HexPathIdComponent> _hexPathIds;

        private EntityPathValidator<HexPortalsPath, HexPathIdComponent, ClearHexPathTag> _validator;

        private readonly INavigationMap _map;
        private readonly PortalPathConstructionProcessManager _processesManager;
        private readonly HexPortalsCoordinator _portalsCoordinator;
        private readonly NavigationGridHandler _navHandler;

        private const int MAX_PROCESSES = 4;


        [Inject]
        public HexPortalPathCalculationSystem(
            INavigationMap map,
            HexPortalsCoordinator portalsCoordinator,
            IPortalsHandler portalsLogic,
            NavigationGridHandler navGridHandler)
        {
            _map = map;
            _portalsCoordinator = portalsCoordinator;
            _navHandler = navGridHandler;
            _processesManager = new PortalPathConstructionProcessManager(Allocator.Persistent, _map, MAX_PROCESSES, _portalsCoordinator, portalsLogic);
        }

        public override void OnAwake() 
        { 
            _filter = World.Filter.With<HexPathCalculationRequestTag>().Build();

            _calculationTags = World.GetStash<HexPathCalculationRequestTag>();
            _progressionComponents = World.GetStash<HexPathProgressionComponent>();

            _triangularPosComponents = World.GetStash<TriangularPosComponent>();
            _moveTargets = World.GetStash<MoveTargetComponent>();
            _hexPathIds = World.GetStash<HexPathIdComponent>();

            _validator = new EntityPathValidator<HexPortalsPath, HexPathIdComponent, ClearHexPathTag>(World, PathStatuses, _portalsCoordinator.GetPortalPaths());
        }

        public override void Dispose()
        {
            _processesManager.Dispose();
        }

        protected override void OnEntityPathCalculated(Entity entity, HexPortalsPath path)
        {
            var firstPortalId = path.Points[0];
            _progressionComponents.Add(entity, new(path.NodesCount, _navHandler.GetTargetHexCoord(entity, firstPortalId)));
            _calculationTags.Remove(entity);
        }

        protected override void OnEntityPathFailed(Entity entity, HexPortalsPath path)
        {
            _calculationTags.Remove(entity);
        }

        protected override bool TryStartCalculation(Entity entity, HexPortalsPath path, out PathCalculationProcessToken token)
        {
            var startTripos = _triangularPosComponents.Get(entity).Value;
            var endTripos = _moveTargets.Get(entity).TriangularPos;
            var endpoints = path.DestinationKeys;

            if (!_map.ContainsHex(endpoints.start.HexCoord) || !_map.ContainsHex(endpoints.end.HexCoord))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"invalid hex destination: {endpoints.start.HexCoord} -> {endpoints.end.HexCoord}");
#endif
                OnPathCalculationFailed(path.Id);

                token = PathCalculationProcessToken.Invalid;
                return false;
            }

            var request = new HexPathSearchRequest(
                startTripos,
                endTripos,
                endpoints.start,
                endpoints.end);

            var launchData = new PortalConstructionProcessInput() { Request = request, ReservedPathId = _hexPathIds.Get(entity).PathId };
            token = _processesManager.TryLaunchProcess(launchData);
            return token.IsValid;
        }
    }
}