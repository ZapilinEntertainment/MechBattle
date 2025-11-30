using System;

namespace ZE.MechBattle
{
    public enum LoadingProcessState : byte { NotStarted = 0, InProgress, Succeed, Failed, Disposed}
    public class LoadingProcess : IDisposable
    {
        public LoadingProcessState State { get; private set; }
        public readonly IDisposable LoadSubscription;    

        public LoadingProcess(IDisposable subscription) 
        {
            LoadSubscription = subscription;
            State = LoadingProcessState.InProgress;
        }

        public void Finish(bool success) 
        {
            if (State == LoadingProcessState.InProgress)
                State = success ? LoadingProcessState.Succeed : LoadingProcessState.Failed;
        }

        public void Dispose() 
        {
            if (State == LoadingProcessState.Disposed)
                return;
            State = LoadingProcessState.Disposed;
            LoadSubscription?.Dispose();
        }
    }
}
