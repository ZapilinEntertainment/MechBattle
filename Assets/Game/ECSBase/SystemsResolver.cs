using System.Collections.Generic;
using UnityEngine;
using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle.Ecs
{
    public class SystemsResolver
    {
        public IReadOnlyDictionary<SystemGroupOrder, SystemsGroup> SystemGroups => _systemGroups;

        private readonly Dictionary<SystemGroupOrder, SystemsGroup> _systemGroups;
        private readonly IObjectResolver _resolver;
        private readonly World _world;

        public SystemsResolver(IObjectResolver resolver)
        {
            _systemGroups = new();
            _resolver = resolver;
            _world = _resolver.Resolve<World>();
        }

        public void AddSystem<T>(SystemGroupOrder order) where T : class, ISystem
        {
            var system = _resolver.Resolve<T>();
            GetGroup(order).AddSystem(system);
        }

        public void AddInitializer<T>(SystemGroupOrder order) where T : class, IInitializer
        {
            var initializer = _resolver.Resolve<T>();
            GetGroup(order).AddInitializer(initializer);
        }

        public void ApplySystems()
        {
            foreach (var groupKvp in _systemGroups)
            {
                _world.AddSystemsGroup((int)groupKvp.Key, groupKvp.Value);
            }

            _world.Commit();
        }

        private SystemsGroup GetGroup(SystemGroupOrder order)
        {
            if (_systemGroups.TryGetValue(order, out var group))
                return group;

            group = _world.CreateSystemsGroup();
            _systemGroups.Add(order, group);
            return group;
        }
    }
}
