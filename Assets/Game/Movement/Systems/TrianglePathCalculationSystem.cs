using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Navigation;


namespace ZE.MechBattle.Ecs
{
    public enum CalculationProcessStage : byte { Idle, Calculating, Complete }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TrianglePathCalculationSystem : ISystem 
    {
        public World World { get; set;}        
        private Filter _startProcessFilter;
        private Filter _completeProcessFilter;
        private Stash<CalculatingTrianglePathComponent> _endpoints;
        private Stash<RegularTrianglePathComponent> _regularPaths;
        private Stash<TrianglePathProcessingComponent> _processingComponents;

        private readonly INavigationMap _map;
        private readonly NavigationTrianglePathsBuffer _pathsBuffer;
        private readonly TrianglePathCalculationProcess[] _calculationProcesses;
        private readonly HashSet<int> _processingPaths = new();

        private readonly Allocator _processAllocator = Allocator.Persistent;
        private const int MAX_PARALLEL_CALCULATIONS = 4;        

        [Inject]
        public TrianglePathCalculationSystem(INavigationMap map, NavigationTrianglePathsBuffer pathsBuffer)
        {
            _map = map;
            _calculationProcesses = new TrianglePathCalculationProcess[MAX_PARALLEL_CALCULATIONS];
            _pathsBuffer = pathsBuffer;
        }

        public void OnAwake() 
        {
            _startProcessFilter = World.Filter
                .With<CalculatingTrianglePathComponent>()
                .Without<FlowTrianglePathComponent>()
                .Without<TrianglePathProcessingComponent>()
                .Build();

            _completeProcessFilter = World.Filter
                .With<TrianglePathProcessingComponent>()
                .Build();

            _endpoints = World.GetStash<CalculatingTrianglePathComponent>();
            _regularPaths = World.GetStash<RegularTrianglePathComponent>();
            _processingComponents = World.GetStash<TrianglePathProcessingComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            var idleProcesses = 0;
            for (var i = 0; i < _calculationProcesses.Length; i++)
            {
                var calculationProcess = _calculationProcesses[i];
                if (calculationProcess == null)
                {
                    idleProcesses++;
                    continue;
                }

                switch (calculationProcess.Stage)
                {
                    case CalculationProcessStage.Complete:
                        {
                            _pathsBuffer.FulfillReservedPath(calculationProcess.PathId, calculationProcess.Stop());
                            idleProcesses++;
                            break;
                        }
                    case CalculationProcessStage.Idle:
                        {
                            idleProcesses++;
                            break;
                        }
                }
            }

            if (idleProcesses != 0) 
                UpdateProcesses();

            CheckProcessingEntities();
        }

        private void UpdateProcesses()
        {
            var startIndex = 0;
            foreach (var entity in _startProcessFilter)
            {
                var component = _endpoints.Get(entity);
                if (_pathsBuffer.TryGetPathShortData(component.Start, component.End, out var shortPathData))
                {
                    AssignPath(entity, shortPathData);
                    continue;
                }

                if (!TryGetNextIdleProcessIndex(startIndex, out var idleProcessIndex))
                    break;

                var idleProcess = _calculationProcesses[idleProcessIndex];
                idleProcess.Launch(_pathsBuffer.ReservePathId(), component.Start, component.End);
                startIndex++;
                _processingComponents.Set(entity, new(idleProcessIndex, idleProcess.ProcessIteration, idleProcess.PathId));
            }
        }

        private void CheckProcessingEntities()
        {
            foreach (var entity in _completeProcessFilter)
            {
                var processingComponent = _processingComponents.Get(entity);
                var process = _calculationProcesses[processingComponent.ProcessIndex];

                // if other calculation set or path have already been calculated
                if (process.ProcessIteration != processingComponent.ProcessIteration)
                {
                    _processingComponents.Remove(entity);
                    var pathId = processingComponent.PathId;
                    if (_pathsBuffer.TryGetPathShortData(pathId, out var shortPathData))
                        AssignPath(entity, new(pathId: pathId, trianglesCount: shortPathData.TrianglesCount));
                }
            }
        }

        public void Dispose()
        {
            foreach (var process in _calculationProcesses)
            {
                process?.Dispose();
            }
        }

        private bool TryGetNextIdleProcessIndex(int startIndex, out int idleProcessIndex)
        {
            var length = _calculationProcesses.Length;
            if (startIndex >= length)
            {
                idleProcessIndex = -1;
                return false;
            }

            for (var i = startIndex; i < length; i++)
            {
                var process = GetOrCreateCalculationProcess(i);

                if (process.Stage == CalculationProcessStage.Idle)
                {
                    idleProcessIndex = i;
                    return true;
                }
            }

            idleProcessIndex = -1;
            return false;
        }

        private void AssignPath(Entity entity, TrianglePathShortData shortData)
        {
            _regularPaths.Set(entity, new(pathId: shortData.PathId, shortData.TrianglesCount));
            _endpoints.Remove(entity);
        }

        private TrianglePathCalculationProcess GetOrCreateCalculationProcess(int index)
        {
            var process = _calculationProcesses[index];
            if (process == null)
            {
                process = new TrianglePathCalculationProcess(_processAllocator, _map);
                _calculationProcesses[index] = process;
            }
            return process;
        }
    }
}