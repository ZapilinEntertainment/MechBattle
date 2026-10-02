using System.Collections.Generic;
using UnityEngine;
using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle.Ecs
{
    public class MorpehSystemInstallHandler
    {
        private readonly struct LateSystem
        {
            public readonly SystemGroupOrder GroupOrder;
            public readonly ISystem System;

            public LateSystem(SystemGroupOrder systemGroupOrder, ISystem system)
            {
                System = system;
                GroupOrder = systemGroupOrder;
            }
        }

        public IReadOnlyDictionary<SystemGroupOrder, SystemsGroup> SystemGroups => _systemGroups;

        private readonly Dictionary<SystemGroupOrder, SystemsGroup> _systemGroups;
        private readonly List<LateSystem> _lateSystems;
        private readonly IObjectResolver _resolver;
        private readonly World _world;

        [Inject]
        public MorpehSystemInstallHandler(World world, IObjectResolver resolver)
        {
            _systemGroups = new();
            _lateSystems = new();
            _resolver = resolver;
            _world = world;
        }

        public void AddSystem<T>(SystemGroupOrder order) where T : class, ISystem
        {
            var system = _resolver.Resolve<T>();
            GetGroup(order).AddSystem(system);
        }

        public void AddLateSystem<T>(SystemGroupOrder order) where T : class, ISystem
        {
            var system = _resolver.Resolve<T>();
            _lateSystems.Add(new(order, system));
        }


        private SystemsGroup GetGroup(SystemGroupOrder order)
        {
            if (_systemGroups.TryGetValue(order, out var group))
                return group;

            group = _world.CreateSystemsGroup();
            _systemGroups.Add(order, group);
            return group;
        }

        public void ApplySystems()
        {
            foreach (var delayedSystem in _lateSystems)
            {
                GetGroup(delayedSystem.GroupOrder).AddSystem(delayedSystem.System);
            }

            foreach (var groupKvp in _systemGroups)
            {
                _world.AddSystemsGroup((int)groupKvp.Key, groupKvp.Value);
            }           

            _world.Commit();
        }
    }
}
