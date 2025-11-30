using System;
using System.Collections.Generic;
using UnityEngine;
using R3;
using ZE.Flags;

namespace ZE.Flags
{

    public class FlagsManager : IFlagsManager
    {
        private readonly Dictionary<Type, IFlagAgent> _agents = new();

        public bool IsFlagActive<T>() where T : IFlag
        {
            var type = typeof(T);
            if (_agents.TryGetValue(type, out var agent))
                return agent.IsFlagActive;

            return false;
        }

        public IDisposable Subscribe<T>(Action<bool> onNext) where T : IFlag =>
            GetOrCreateAgent<T>().Subscribe(onNext);

        public T AddFlag<T>() where T : IFlag
        {
            var instance = Activator.CreateInstance<T>();
            AddFlag(instance);
            return instance;
        }

        public void AddFlag<T>(T instance) where T : IFlag => GetOrCreateAgent<T>().AddFlag(instance);

        public void RemoveFlag<T>(T instance) where T : IFlag
        {
            var type = typeof(T);
            if (_agents.TryGetValue(type, out var agent))
                agent.RemoveFlag(instance);
        }

        public IDisposable AddTemporalFlag<T>() where T : IFlag
        {
            var flag = AddFlag<T>();
            return Disposable.Create(() => this.RemoveFlag(flag));
        }

        public T GetFirstFlag<T>() where T : IFlag
        {
            var type = typeof(T);
            if (!_agents.TryGetValue(type, out var agent))
                return default;
            return (agent as FlagAgent<T>).GetFirstFlag();
        }

        public void Dispose()
        {
            foreach (var agent in _agents.Values)
            {
                agent.Dispose();
            }
            _agents.Clear();
        }

        private IFlagAgent GetOrCreateAgent<T>() where T : IFlag
        {
            var type = typeof(T);
            if (!_agents.TryGetValue(type, out var agent))
            {
                agent = new FlagAgent<T>();
                _agents[type] = agent;
            }
            return agent;
        }
    }
}
