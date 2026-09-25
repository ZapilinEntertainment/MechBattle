using System.Buffers;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using ZE.Utils;

namespace ZE.MechBattle.Ecs {

    public enum PathCalculationStatus : byte
    {
        Undefined, Calculating, Completed, Invalid
    }

    public interface ICalculationSystemPath
    {
        int Id { get; }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class PathCalculationSystemBase<PathType> : ISystem 
        where PathType : ICalculationSystemPath, ILRUBufferElement
    {
        public World World { get; set; }
        protected abstract int MAX_CACHED_STATUSES_COUNT { get; }
        protected abstract Filter Filter { get; }
        protected abstract IProcessManager<PathCalculationProcessToken> ProcessManager { get; }
        protected abstract IEntityPathValidator<PathType> PathValidator { get; }


        protected readonly Dictionary<int, PathCalculationStatus> PathStatuses;
        private readonly Dictionary<int, PathCalculationProcessToken> _calculationProcessTokens = new();
        private readonly ArrayPool<int> _pool;

        public PathCalculationSystemBase()
        {
            PathStatuses = new(MAX_CACHED_STATUSES_COUNT);
            _pool = ArrayPool<int>.Shared;
        }

        public abstract void OnAwake();
        public abstract void Dispose();

        public void OnUpdate(float deltaTime)
        {
            HandleActiveProcesses();
            var idleProcessesCount = ProcessManager.UpdateAndGetIdleProcessesCount();
            HandleReceivedRequests(idleProcessesCount);
        }        

        protected void OnPathCalculationFailed(int pathId)
        {
            _calculationProcessTokens.Remove(pathId);
            PathStatuses[pathId]= PathCalculationStatus.Invalid;
        }

        protected abstract void OnEntityPathFailed(Entity entity, PathType path);
        protected abstract void OnEntityPathCalculated(Entity entity, PathType path);

        protected abstract bool TryStartCalculation(Entity entity, PathType path, out PathCalculationProcessToken token);

        protected virtual void OnImpossibleRequestFound(Entity entity, PathType path)
        {
#if UNITY_EDITOR
            // this is unexpected behaviour
            UnityEngine.Debug.LogWarning($"cannot start new process at {GetType().Name} for some reason");
#endif
        }

        private void HandleActiveProcesses()
        {
            if (_calculationProcessTokens.Count == 0)
                return;


            var clearPositions = 0;
            var clearArray = _pool.Rent(_calculationProcessTokens.Count);
            foreach (var processTokensKvp in _calculationProcessTokens)
            {
                if (ProcessManager.IsProcessCompleted(processTokensKvp.Value))
                {
                    clearArray[clearPositions++] = processTokensKvp.Key;
                }
            }

            for (var i = 0; i < clearPositions; i++)
            {
                var pathId = clearArray[i];
                _calculationProcessTokens.Remove(pathId);
                PathStatuses[pathId]  = PathCalculationStatus.Completed;
            }

            _pool.Return(clearArray);
        }


        private void HandleReceivedRequests(int idleProcessesCount)
        {
            foreach (var entity in Filter)
            {
                if (!PathValidator.ValidateAndGetCalculationStatus(entity, out var status, out var path))
                    continue;

                switch (status)
                {
                    case PathCalculationStatus.Completed:                   
                        {
                           OnEntityPathCalculated(entity, path);
                           break;
                        }
                    case PathCalculationStatus.Invalid:
                        {
                            OnEntityPathFailed(entity, path);
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
                            
                            if (!TryStartCalculation(entity, path, out var token))
                            {
                                OnImpossibleRequestFound(entity, path);
                                continue;
                            }

                            var pathId = path.Id;
                            _calculationProcessTokens.Add(pathId, token);
                            PathStatuses[pathId] = PathCalculationStatus.Calculating;
                            idleProcessesCount--;
                            break;
                        }
                }
            }
        }
    }
}