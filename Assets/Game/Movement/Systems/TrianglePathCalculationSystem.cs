using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Navigation;


namespace ZE.MechBattle.Ecs
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TrianglePathCalculationSystem : ISystem 
    {
        public World World { get; set;}        
        private Filter _startProcessFilter;
        private Filter _processingEntitiesFilter;

        private Stash<CalculatingTrianglePathComponent> _endpoints;
        private Stash<RegularTrianglePathComponent> _regularPaths;
        private Stash<TrianglePathProcessingComponent> _processingComponents;
        private Stash<ClearHexPathTag> _clearHexPathTag;

        private readonly INavigationMap _map;
        private readonly TrianglePathCalculationProcessManager _processesManager;
        private readonly TrianglePathsLRUBuffer _pathsList;    

        private const int MAX_PROCESSES_COUNT = 4;

        [Inject]
        public TrianglePathCalculationSystem(INavigationMap map, TrianglePathsLRUBuffer trianglePathsBuffer)
        {
            _map = map;
            _pathsList = trianglePathsBuffer;
            _processesManager = new TrianglePathCalculationProcessManager(Allocator.Persistent, _map, MAX_PROCESSES_COUNT, _pathsList);
        }

        public void OnAwake() 
        {
            _startProcessFilter = World.Filter
                .With<CalculatingTrianglePathComponent>()
                .Without<FlowTrianglePathComponent>()
                .Without<TrianglePathProcessingComponent>()
                .Build();

            _processingEntitiesFilter = World.Filter
                .With<TrianglePathProcessingComponent>()
                .Build();

            _endpoints = World.GetStash<CalculatingTrianglePathComponent>();
            _regularPaths = World.GetStash<RegularTrianglePathComponent>();
            _processingComponents = World.GetStash<TrianglePathProcessingComponent>();
            _clearHexPathTag = World.GetStash<ClearHexPathTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            var idleProcessCount = _processesManager.UpdateAndGetIdleProcessesCount();
            if (idleProcessCount != 0) 
                UpdateAwaitingEntities(idleProcessCount);

            CheckProcessingEntities();
        }

        private void UpdateAwaitingEntities(int idleProcessCounts)
        {
            foreach (var entity in _startProcessFilter)
            {
                var endpoints = _endpoints.Get(entity);
                if (_pathsList.TryGetPathByEndpoints(endpoints.Start, endpoints.End, out var path))
                {
                    AssignPath(entity, path);
                    continue;
                }

                var processToken = _processesManager.TryLaunchProcess(new(endpoints.Start, endpoints.End));
                if (!processToken.IsValid)
                {
                    #if UNITY_EDITOR
                    UnityEngine.Debug.LogWarning("process manager idle process count was invalid");
                    #endif
                    return;
                }
                
                idleProcessCounts--;
                _processingComponents.Set(entity, new(processToken.PathId));

                if (idleProcessCounts == 0)
                    return;
            }
        }

        private void CheckProcessingEntities()
        {
            foreach (var entity in _processingEntitiesFilter)
            {
                var pathId = _processingComponents.Get(entity).PathId;

                if (!_pathsList.TryGetPath(pathId, out var path))
                {
                    _clearHexPathTag.Set(entity);
                    continue;
                }

                if (path.IsCalculated)
                {
                    _processingComponents.Remove(entity);
                    AssignCalculatedPath(entity, path);
                }
            }
        }

        public void Dispose()
        {
            _processesManager.Dispose();
        }

        private void AssignPath(Entity entity, PathData<IntTriangularPos, IntTriangularPos> path)
        {
            if (path.IsCalculated)
                AssignCalculatedPath(entity, path);
            else
                _processingComponents.Set(entity, new(path.Id));

            _endpoints.Remove(entity);
        }

        private void AssignCalculatedPath(Entity entity, PathData<IntTriangularPos, IntTriangularPos> path)
        {
            _regularPaths.Set(entity, new(pathId: path.Id, path.NodesCount));
        }
    }
}