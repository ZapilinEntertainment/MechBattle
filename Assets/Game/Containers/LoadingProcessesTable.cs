using System;
using System.Collections.Generic;

namespace ZE.MechBattle
{
    public class LoadingProcessesTable : IDisposable
    {
        private readonly Dictionary<Guid, LoadingProcess> _processes = new();

        /// <summary>
        /// NOTE: subscription will be disposed when disposing the whole table (forced stop). 
        /// It wont be disposed on remove
        /// </summary>
        public LoadingToken RegisterProcess(IDisposable subscription) 
        {
            var guid = Guid.NewGuid();
            var process = new LoadingProcess(subscription);
            _processes.Add(guid, process);
            return new() { IsCompleted = false, ProcessGuid = guid};            
        }

        public void RemoveProcess(LoadingToken token) => _processes.Remove(token.ProcessGuid);

        public void Dispose()
        {
            foreach (var process in _processes.Values) process.Dispose();
            _processes.Clear();
        }

        public LoadingProcessState GetLoadingProcessState(Guid guid) => _processes.TryGetValue(guid, out var process) ? process.State : LoadingProcessState.NotStarted;
    
        public bool IsProcessCompleted(Guid guid)
        {
            var state = GetLoadingProcessState(guid);
            return state != LoadingProcessState.NotStarted && state !=LoadingProcessState.InProgress;
        }
    }
}
