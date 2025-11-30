using System;
using R3;

public interface IFlag { }

public interface IFlagsManager : IDisposable
{
    bool IsFlagActive<T>() where T : IFlag ;

    IDisposable Subscribe<T>(Action<bool> onNext) where T : IFlag ;

    T AddFlag<T>() where T : IFlag;

    void AddFlag<T>(T instance) where T : IFlag;

    void RemoveFlag<T>(T instance) where T : IFlag;

    IDisposable AddTemporalFlag<T>() where T : IFlag;

    T GetFirstFlag<T>() where T : IFlag;
}
