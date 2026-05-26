using UnityEngine;

namespace ZE.Utils
{
    public abstract class AsyncProcessBase<Input> : IProcess
    {
        public CalculationProcessStage Stage { get;private set; }
        public int ProcessIteration { get; private set; }
        protected bool StopProcessRequired { get;private set;} = false;
        protected abstract bool IsDisposeAvailable { get; }

        public async void LaunchAsync(Input input)
        {
            Stage = CalculationProcessStage.Calculating;
            await ExecuteAsync(input);
            Stage = CalculationProcessStage.Complete;
            ProcessIteration++;
        }

        public async void Dispose()
        {
            // note: async process can use job processes inside, so they cannot be stop immediately (because of job collections)
            StopProcessRequired = true;
            while (!IsDisposeAvailable)
            {
                await Awaitable.NextFrameAsync();
            }
            DisposeResources();
        }

        protected abstract Awaitable ExecuteAsync(Input input);
        protected abstract void DisposeResources();
    }
}
