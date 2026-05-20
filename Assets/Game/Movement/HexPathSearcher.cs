using System;
using System.Collections.Generic;
using ZE.MechBattle.Navigation;
using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public class HexPathSearcher
    {
        private readonly HexPathsLRUBuffer _hexPathsBuffer;
        private readonly MovementTasksFactory _movementTasks;
        private readonly HexPathsSearchHistory _hexPathsCache;

        [Inject]
        public HexPathSearcher(HexPathsLRUBuffer hexPaths, MovementTasksFactory movementTasks, HexPathsSearchHistory hexPathsCache)
        {
            _hexPathsBuffer = hexPaths;
            _movementTasks = movementTasks;
            _hexPathsCache = hexPathsCache;
        }


        public HexPathSearchResultData TryGetHexPath(in HexPathSearchRequest searchRequest)
        {
            if (_hexPathsCache.TryGetCachedSolution(searchRequest, out var path))
            { 
                if (!path.IsCalculated)
                    return new() { PathId = path.Id, Result = HexPathSearchResult.CalculationNotFinished };

                return new()
                {
                    PathId = path.Id,
                    NodesCount = path.NodesCount,
                    EndNode = path.LastNode,
                    Result = path.HasReachedTarget ? HexPathSearchResult.OnlyIncompletePathPossible : HexPathSearchResult.PathFound
                };
            }

            return new()
            {
                Result = HexPathSearchResult.CalculationNotFinished,
                ConstructionAwaitingToken = _movementTasks.RequestHexPathCalculation(searchRequest)
            };
        }
    }
}
