using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Navigation;
using ZE.Utils;

namespace ZE.MechBattle.Ecs
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TrianglePathCalculationSystem : PathCalculationSystemBase<TrianglesPath>
    {
        // same as Hex portal path calculation system

        protected override int MAX_CACHED_STATUSES_COUNT => 64;
        protected override Filter Filter => _filter;

        protected override IProcessManager<PathCalculationProcessToken> ProcessManager => _processesManager;
        protected override IEntityPathValidator<TrianglesPath> PathValidator => _pathValidator;

        private Filter _filter;
        private Stash<RegularTrianglePathProgressionComponent> _progressionComponents;
        private Stash<TrianglePathCalculationTag> _calculationTags;
        private Stash<RegularTrianglePathComponent> _tripathComponents;
        private EntityPathValidator<TrianglesPath, RegularTrianglePathComponent, ClearTrianglePathTag> _pathValidator;

        private readonly TrianglePathCalculationProcessManager _processesManager;
        private readonly TrianglePathsLRUBuffer _paths;
        private const int MAX_PROCESSES_COUNT = 4;

        [Inject]
        public TrianglePathCalculationSystem(INavigationMap map, TrianglePathsLRUBuffer paths)
        {
            _paths = paths;
            _processesManager = new(Allocator.Persistent, map, MAX_PROCESSES_COUNT, _paths);
        }

        public override void Dispose()
        {
            _processesManager.Dispose();
        }

        public override void OnAwake()
        {
            _filter = World.Filter.With<TrianglePathCalculationTag>().Build();

            _progressionComponents = World.GetStash<RegularTrianglePathProgressionComponent>();
            _calculationTags = World.GetStash<TrianglePathCalculationTag>();
            _tripathComponents = World.GetStash<RegularTrianglePathComponent>();

            _pathValidator = new(World,  PathStatusesLRU, _paths);
        }

        protected override void OnPathCalculated(Entity entity, TrianglesPath path)
        {
            _progressionComponents.Set(entity, new(path.NodesCount));
            _calculationTags.Remove(entity);
        }

        protected override bool TryStartCalculation(Entity entity, TrianglesPath path, out PathCalculationProcessToken token)
        {
            var endpoints = path.DestinationKeys;
            var id = _tripathComponents.Get(entity).PathId;
            token = _processesManager.TryLaunchProcess(new(id, endpoints.start, endpoints.end));
            return token.IsValid;
        }
    }
}