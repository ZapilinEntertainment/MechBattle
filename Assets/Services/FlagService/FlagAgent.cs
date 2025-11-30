using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

namespace ZE.Flags
{
    public interface IFlagAgent : IDisposable
    {
        bool IsFlagActive => FlagActiveProperty.CurrentValue;
        ReadOnlyReactiveProperty<bool> FlagActiveProperty { get; }

        void AddFlag(IFlag flag);
        void RemoveFlag(IFlag flag);
        IDisposable Subscribe(Action<bool> onNext);

    }

    public class FlagAgent<T> : IFlagAgent where T : IFlag
    {
        public ReadOnlyReactiveProperty<bool> FlagActiveProperty => _isFlagActiveProperty;
        private readonly HashSet<IFlag> _flags = new();
        private readonly ReactiveProperty<bool> _isFlagActiveProperty = new(false);

        public void AddFlag(IFlag flag)
        {
            _flags.Add(flag);
            _isFlagActiveProperty.Value = true;
        }

        public void RemoveFlag(IFlag flag)
        {
            _flags.Remove(flag);
            _isFlagActiveProperty.Value = _flags.Count != 0;
        }

        public void Dispose()
        {
            _isFlagActiveProperty.Value = false;
            _flags.Clear();
            _isFlagActiveProperty.Dispose();
        }

        public IDisposable Subscribe(Action<bool> onNext) => _isFlagActiveProperty.Subscribe(onNext);

        public T GetFirstFlag()
        {
            if (_flags.Count == 0)
                return default;

            var enumerator = _flags.GetEnumerator();
            enumerator.MoveNext();
            return (T)enumerator.Current;
        }
    }
}
