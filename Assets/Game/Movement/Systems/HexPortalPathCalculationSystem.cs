using System.Buffers;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using Unity.Collections;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPortalPathCalculationSystem : ISystem 
    {
        private enum PathCalculationStatus : byte
        {
            Undefined, Calculating, Completed
        }

        public World World { get; set;}
        private Filter _filter;
        private Stash<HexPathCalculationRequestTag> _calculationTags;
        private Stash<HexPathComponent> _pathComponents;
        private Stash<ClearHexPathTag> _pathClearTags;
        private Stash<HexPathProgressionComponent> _progressionComponents;
        private Stash<TriangularPosComponent> _triangularPosComponents;
        private Stash<MoveTargetComponent> _moveTargets;

        private readonly INavigationMap _map;
        private readonly HexPortalPathsLRUBuffer _portalPaths;
        private readonly PortalPathConstructionProcessManager _processesManager;
        private readonly LRUDictionaryCache<int, PathCalculationStatus> _pathStatusesLRU = new(MAX_CACHED_STATUSES_COUNT);
        private readonly Dictionary<int, PathCalculationProcessToken> _calculationProcessTokens = new();
        private readonly ArrayPool<int> _pool;
        private const int MAX_PORTAL_PROCESSES = 4;
        private const int MAX_CACHED_STATUSES_COUNT = 64;


        [Inject]
        public HexPortalPathCalculationSystem(
            HexPortalPathsLRUBuffer portalPaths,
            INavigationMap map,
            PortalConnectionsList portalConnectionsList)
        {
            _map = map;
            _portalPaths = portalPaths;
            _processesManager = new PortalPathConstructionProcessManager(Allocator.Persistent, _map, portalConnectionsList, MAX_PORTAL_PROCESSES, portalPaths);
        
            _pool = ArrayPool<int>.Shared;
        }

        public void OnAwake() 
        { 
            _filter = World.Filter.With<HexPathCalculationRequestTag>().Build();

            _calculationTags = World.GetStash<HexPathCalculationRequestTag>();
            _pathComponents = World.GetStash<HexPathComponent>();
            _pathClearTags = World.GetStash<ClearHexPathTag>();
            _progressionComponents = World.GetStash<HexPathProgressionComponent>();

            _triangularPosComponents = World.GetStash<TriangularPosComponent>();
            _moveTargets = World.GetStash<MoveTargetComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            HandleActiveProcesses();
            var idleProcessesCount = _processesManager.UpdateAndGetIdleProcessesCount();
            HandleReceivedRequests(idleProcessesCount);
        }

        public void Dispose() { }

        private void HandleActiveProcesses()
        {
            if (_calculationProcessTokens.Count == 0)
                return;

             
            var clearPositions = 0;
            var clearArray = _pool.Rent(_calculationProcessTokens.Count);
            foreach (var processTokensKvp in _calculationProcessTokens)
            {
                if (_processesManager.IsProcessCompleted(processTokensKvp.Value))
                {
                    clearArray[clearPositions++] = processTokensKvp.Key;
                }
            }

            for (var i = 0; i < clearPositions; i++)
            {
                var pathId = clearArray[i];
                _calculationProcessTokens.Remove(pathId);
                _pathStatusesLRU.AddCachedValue(pathId, PathCalculationStatus.Completed);
            }

            _pool.Return(clearArray);
        }


        private void HandleReceivedRequests(int idleProcessesCount)
        {
            foreach (var entity in _filter)
            {
                var pathId = _pathComponents.Get(entity).PathId;
                var currentPathStatus = _pathStatusesLRU.TryGetCachedValue(pathId, out var status) ? status : PathCalculationStatus.Undefined;
                if (!_portalPaths.TryGetValue(pathId, out var path, updateUsingTime: true))
                {
                    _pathClearTags.Add(entity);
                    continue;
                }

                switch (currentPathStatus)
                {
                    case PathCalculationStatus.Completed:
                        {
                            _progressionComponents.Add(entity, new(path.NodesCount));
                            _calculationTags.Remove(entity);
                            break;
                        }
                    case PathCalculationStatus.Calculating:
                        {
                            continue;
                        }
                    default:
                        {
                            // path status undefined
                            if (idleProcessesCount == 0)
                                continue;

                            var startTripos = _triangularPosComponents.Get(entity).Value;
                            var endTripos = _moveTargets.Get(entity).TriangularPos;
                            var endpoints = path.DestinationKeys;

                            var request = new HexPathSearchRequest(
                                startTripos,
                                endTripos,
                                endpoints.start,
                                endpoints.end);

                            var token = _processesManager.TryLaunchProcess(request);
                            if (!token.IsValid)
                            {
                                idleProcessesCount = 0;
                                continue;
                            }

                            _calculationProcessTokens.Add(pathId, token);  
                            _pathStatusesLRU.AddCachedValue(pathId, PathCalculationStatus.Calculating);
                            idleProcessesCount--;
                            break;
                        }
                }
            }
        }
    }
}