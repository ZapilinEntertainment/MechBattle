using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs 
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPathCalculationSystem : ISystem 
    {
        private readonly struct CachedOperationKey
        {
            public readonly HexEdgesMask StartHexMask;
            public readonly HexEdgesMask EndHexMask;
            public readonly int2 StartHexCoord;
            public readonly int2 EndHexCoord;

            public CachedOperationKey(int2 startHexCoord, HexEdgesMask startEdgesMask, int2 endHexCoord, HexEdgesMask endEdgesMask)
            {
                StartHexCoord = startHexCoord;
                EndHexCoord = endHexCoord;
                StartHexMask = startEdgesMask;
                EndHexMask = endEdgesMask;
            }
        }

        public readonly struct CachedSearchResult
        {
            public readonly bool IsFinalResult;
            public readonly int PathId;
            public readonly int PathNodesCount;

            public CachedSearchResult(int pathId, int nodesCount)
            {
                PathId = pathId;
                IsFinalResult = true;
                PathNodesCount = nodesCount;
            }
        }

        public World World { get; set;}
        private readonly HexPathsLRUBuffer _pathsList;
        private readonly INavigationMap _map;  
        private readonly PathCalculationProcessesManager<HexPathNodeKey> _processesManager;
        private readonly HashSet<HexPathKey> _awaitingCalculations = new();

        private readonly LRUDictionaryCache<CachedOperationKey, CachedSearchResult> _cachedResults;
        private int _lastAppliedMapVersion;

        private Filter _requestsFilter;        
        private Stash<HexPathSelectRequestComponent> _selectComponentsStash;
        private Stash<RegularHexPathComponent> _regularPathStash;

        private const int MAX_PARALLEL_CALCULATIONS = 4;
        private const int CACHE_LIMIT = 32;

        [Inject]
        public HexPathCalculationSystem(HexPathsLRUBuffer list, INavigationMap map)
        {
            _pathsList = list;
            _map = map;
            _processesManager = new(MAX_PARALLEL_CALCULATIONS, _pathsList);
            _lastAppliedMapVersion = map.Version;
            _cachedResults = new (CACHE_LIMIT);
        }

        public void OnAwake() 
        { 
            _requestsFilter = World.Filter
                .With<HexPathSelectRequestComponent>()
                .Without<ClearHexPathTag>()
                .Build();

            _selectComponentsStash = World.GetStash<HexPathSelectRequestComponent>();
            _regularPathStash = World.GetStash<RegularHexPathComponent>();
        }

        public void Dispose()
        {
            _processesManager.Dispose();
        }

        public void OnUpdate(float deltaTime) 
        {            
            if (!_map.IsInitialized)
                return;            
            
            _awaitingCalculations.Clear();           
            HandleRequestingEntities();

            var idleProcessesCount = _processesManager.UpdateAndGetIdleProcessesCount();
            TryStartAwaitingPathsCalculation(idleProcessesCount);
        }

        private void HandleRequestingEntities()
        {
            UpdateMapVersion();

            foreach (var entity in _requestsFilter)
            {
                var requestComponent = _selectComponentsStash.Get(entity);
                var startEdgesMask = requestComponent.StartEdgesMask;
                var endEdgesMask = requestComponent.EndEdgesMask;
                var startHexCoord = requestComponent.StartHex;
                var endHexCoord = requestComponent.EndHex;

                var operationKey = new CachedOperationKey(startHexCoord, startEdgesMask, endHexCoord, endEdgesMask);
                if (_cachedResults.TryGetCachedValue(operationKey, out var cachedResult))
                {
                    if (cachedResult.IsFinalResult)  
                        SetEntityHexPath(entity, cachedResult.PathId, cachedResult.PathNodesCount);
                    continue;
                }


                var minPathCost = float.MaxValue;
                var shortestPathId = -1;
                PathData<HexPathNodeKey> shortestPathData = null;
                var allOptionsCalculated = true;

                for (var startEdge = 0; startEdge < 6; startEdge++)
                {
                    if (!startEdgesMask.IsEdgePresented(startEdge)) 
                        continue;

                    for (var endEdge = 0; endEdge < 6; endEdge++)
                    {
                        if (!endEdgesMask.IsEdgePresented(endEdge))
                            continue;

                        var startNode = new HexPathNodeKey(startHexCoord, startEdge);
                        var endNode = new HexPathNodeKey(endHexCoord, endEdge);
                        if (_pathsList.TryGetPathByEndpoints(startNode, endNode, out var pathId, out var pathData))
                        {
                            if (pathData.PathCost < minPathCost)
                            {
                                shortestPathData = pathData;
                                minPathCost = pathData.PathCost;
                                shortestPathId = pathId;
                            }
                        }
                        else
                        {
                            allOptionsCalculated = false;
                            _awaitingCalculations.Add(new(startNode, endNode));
                        }
                    }
                }

                if (!allOptionsCalculated)
                {
                    _cachedResults.AddCachedValue(operationKey, default);
                    continue;
                }

                var nodesCount = shortestPathData.NodesCount;
                var result = new CachedSearchResult(shortestPathId, nodesCount);
                _cachedResults.AddCachedValue(operationKey, new(shortestPathId, nodesCount));

                SetEntityHexPath(entity, shortestPathId, nodesCount);
            }
        }

        private void SetEntityHexPath(Entity entity, int pathId, int nodesCount)
        {
            _regularPathStash.Set(entity, new RegularHexPathComponent(pathId, nodesCount));
            _selectComponentsStash.Remove(entity);
        }

        private void TryStartAwaitingPathsCalculation(int idleProcessesCount)
        {
            foreach (var request in _awaitingCalculations)
            {
                _processesManager.TryLaunchProcess(request.Start, request.End);
                idleProcessesCount--;
                if (idleProcessesCount == 0)
                    break;
            }
        }

        private void UpdateMapVersion()
        {
            var currentMapVersion = _map.Version;
            if (currentMapVersion != _lastAppliedMapVersion)
            {
                _lastAppliedMapVersion = currentMapVersion;
                _cachedResults.Clear();
            }
        }
    }
}