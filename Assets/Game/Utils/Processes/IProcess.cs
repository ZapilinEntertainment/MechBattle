using System;

namespace ZE.Utils
{
    public enum CalculationProcessStage : byte { Idle, Calculating, Complete }

    public interface IProcess : IDisposable
    {
        CalculationProcessStage Stage { get; }
        int ProcessIteration { get; }
    }
}
