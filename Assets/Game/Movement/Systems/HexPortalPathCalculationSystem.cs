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

        private EntityPathValidator<HexPortalsPath, HexPathComponent, ClearHexPathTag> _validator;

        private readonly INavigationMap _map;
        private readonly HexPortalPathsLRUBuffer _portalPaths;
        private readonly PortalPathConstructionProcessManager _processesManager;

        private const int MAX_PROCESSES = 4;


        [Inject]
        public HexPortalPathCalculationSystem(
            HexPortalPathsLRUBuffer portalPaths,
            INavigationMap map,
            PortalConnectionsList portalConnectionsList)
        {
            _map = map;
            _portalPaths = portalPaths;
            _processesManager = new PortalPathConstructionProcessManager(Allocator.Persistent, _map, portalConnectionsList, MAX_PROCESSES, portalPaths);
        }

        public override void OnAwake() 
        { 
            _filter = World.Filter.With<HexPathCalculationRequestTag>().Build();

            _calculationTags = World.GetStash<HexPathCalculationRequestTag>();
            _progressionComponents = World.GetStash<HexPathProgressionComponent>();

            _triangularPosComponents = World.GetStash<TriangularPosComponent>();
            _moveTargets = World.GetStash<MoveTargetComponent>();

            _validator = new EntityPathValidator<HexPortalsPath, HexPathComponent, ClearHexPathTag>(World, PathStatusesLRU, _portalPaths);
        }

        public override void Dispose()
        {
            _processesManager.Dispose();
        }

        protected override void OnPathCompleted(Entity entity, HexPortalsPath path)
        {
            _progressionComponents.Add(entity, new(path.NodesCount));
            _calculationTags.Remove(entity);
        }

        protected override bool TryStartCalculation(Entity entity, HexPortalsPath path, out PathCalculationProcessToken token)
        {
            var startTripos = _triangularPosComponents.Get(entity).Value;
            var endTripos = _moveTargets.Get(entity).TriangularPos;
            var endpoints = path.DestinationKeys;

            var request = new HexPathSearchRequest(
                startTripos,
                endTripos,
                endpoints.start,
                endpoints.end);

            token = _processesManager.TryLaunchProcess(request);
            return token.IsValid;
        }
    }
}