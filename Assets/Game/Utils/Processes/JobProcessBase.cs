using System;
using UnityEngine;
using Unity.Jobs;

namespace ZE.Utils
{
    public abstract class JobProcessBase<Input, Output> : IProcess
    {
        public CalculationProcessStage Stage => _isLaunched
                ? (_activeHandle.IsCompleted ? CalculationProcessStage.Complete : CalculationProcessStage.Calculating)
                : CalculationProcessStage.Idle;


        public int ProcessIteration { get; private set; }

        private JobHandle _activeHandle;
        private bool _isLaunched;

        public virtual void Launch(Input input)
        {
            _activeHandle = LaunchJob(input);
            _isLaunched = true;
            ProcessIteration++;
        }

        public Output StopAndGetResults()
        {
            _activeHandle.Complete();
            _isLaunched = false;
            ProcessIteration++;
            return FormResults();
        }

        public void Dispose()
        {
            _activeHandle.Complete();
            DisposeResources();
        }

        protected abstract Output FormResults();
        protected abstract JobHandle LaunchJob(Input input);
        protected abstract void DisposeResources();
    }
}
