using System;

namespace ZE.Utils
{
    public interface IProcessManager<Token> where Token : IProcessToken
    {
        int UpdateAndGetIdleProcessesCount();
        bool IsProcessCompleted(Token token);
    }

    public abstract class ProcessManagerBase<Process, ProcessLaunchData, Token> : IDisposable, IProcessManager<Token> 
        where Process: IProcess 
        where Token : IProcessToken
    {
        protected readonly Process[] Processes;

        public ProcessManagerBase(int maxProcessesCount)
        {
            Processes = new Process[maxProcessesCount];
        }

        public void Dispose()
        {
            foreach (var process in Processes)
            {
                process?.Dispose();
            }
        }

        public int UpdateAndGetIdleProcessesCount()
        {
            var idleProcesses = 0;
            for (var i = 0; i < Processes.Length; i++)
            {
                var calculationProcess = Processes[i];
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
            for (var i = 0; i < Processes.Length; i++)
            {
                var process = Processes[i];
                if (process == null)
                {
                    process = CreateNewProcess();
                    Processes[i] = process;
                }

                if (process.Stage == CalculationProcessStage.Idle)
                    return LaunchProcess(launchData, process, i);
            }
            return default;
        }

        public bool IsProcessCompleted(Token token) =>
            !token.IsValid
            || Processes[token.ProcessIndex].ProcessIteration != token.ProcessIteration;


        protected abstract Token LaunchProcess(ProcessLaunchData launchData, Process process, int processIndex);

        protected abstract Process CreateNewProcess();
        protected abstract void HandleResults(Process process);
    }
}
