using System;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public abstract class PathCalculationProcessesManager<NodeKey> : IDisposable where NodeKey : unmanaged
    {
        private readonly PathCalculationProcess<NodeKey>[] _processes;
        private readonly IPathsList<NodeKey> _pathsList;

        public PathCalculationProcessesManager(int maxProcessesCount, IPathsList<NodeKey> pathsList)
        {
            _processes = new PathCalculationProcess<NodeKey>[maxProcessesCount];
            _pathsList = pathsList;
        }

        public void Dispose()
        {
            foreach (var process in _processes)
            {
                process?.Dispose();
            }
        }

        public int UpdateAndGetIdleProcessesCount()
        {
            var idleProcesses = 0;
            for (var i = 0; i < _processes.Length; i++)
            {
                var calculationProcess = _processes[i];
                if (calculationProcess == null)
                {
                    idleProcesses++;
                    continue;
                }

                switch (calculationProcess.Stage)
                {
                    case CalculationProcessStage.Complete:
                        {
                            _pathsList.AddCalculatedPath(calculationProcess.PathId, calculationProcess.StopAndGetResult());
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
            return idleProcesses;
        }


        public PathCalculationProcessToken TryLaunchProcess(NodeKey start, NodeKey end)
        {
            for (var i = 0; i < _processes.Length; i++)
            {
                var process = _processes[i];
                if (process == null)
                {
                    process = CreateNewProcess();
                    _processes[i] = process;
                }

                if (process.Stage == CalculationProcessStage.Idle)
                {
                    var reservedPathId = _pathsList.ReservePathId();
                    process.Launch(reservedPathId, start, end);
                    return new (reservedPathId, i, process.ProcessIteration);
                }
            }
            return default;
        }

        public bool IsProcessCompleted(PathCalculationProcessToken token) => 
            !token.IsValid 
            || _processes[token.ProcessIndex].ProcessIteration != token.ProcessIteration;

        public abstract PathCalculationProcess<NodeKey> CreateNewProcess();
    }
}
