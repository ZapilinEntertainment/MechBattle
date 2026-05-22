using System;
using UnityEngine;
using Unity.Jobs;

namespace ZE.MechBattle
{
    public enum CalculationProcessStage : byte { Idle, Calculating, Complete }

    public interface IProcess<ProcessInput, ProcessOutput> : IDisposable
    {
        CalculationProcessStage Stage { get; }
        int ProcessIteration { get; }
    }

    public abstract class ProcessBase<Input, Output> : IProcess<Input, Output>
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

        public async void Dispose()
        {
            if (_isLaunched)
            {
                while (!_activeHandle.IsCompleted)
                    await Awaitable.NextFrameAsync();
            }
            _activeHandle = default;
            DisposeResources();
        }

        protected abstract Output FormResults();
        protected abstract JobHandle LaunchJob(Input input);
        protected abstract void DisposeResources();
    }
}
