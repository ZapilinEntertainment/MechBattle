using System;

namespace ZE.Utils
{

    public abstract class ProcessManagerBase<Process, ProcessLaunchData, Token> : IDisposable 
        where Process: IProcess 
        where Token : IProcessToken
    {
        private readonly Process[] _processes;

        public ProcessManagerBase(int maxProcessesCount)
        {
            _processes = new Process[maxProcessesCount];
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
                            HandleResults(calculationProcess);
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


        public Token TryLaunchProcess(ProcessLaunchData launchData)
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
                    return LaunchProcess(launchData, process, i);
            }
            return default;
        }

        public bool IsProcessCompleted(Token token) =>
            !token.IsValid
            || _processes[token.ProcessIndex].ProcessIteration != token.ProcessIteration;


        protected abstract Token LaunchProcess(ProcessLaunchData launchData, Process process, int index);

        protected abstract Process CreateNewProcess();
        protected abstract void HandleResults(Process process);
    }
}
