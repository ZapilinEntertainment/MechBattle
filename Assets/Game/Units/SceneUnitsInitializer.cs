using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;


namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SceneUnitsInitializer : IInitializer 
    {
        public World World { get; set;}
        private readonly UnitsFactory _unitsFactory;
        private readonly Stash<UnitGroupComponent> _groups;
        private readonly GroupBuilder _groupBuilder;

        [Inject]
        public SceneUnitsInitializer(UnitsFactory unitsFactory, World world, GroupBuilder groupBuilder)
        {
            _unitsFactory = unitsFactory;
            _groupBuilder = groupBuilder;
            _groups = world.GetStash<UnitGroupComponent>();
        }

        public void OnAwake() 
        {
            var tanks = GameObject.FindObjectsByType<TankView>(FindObjectsSortMode.None);
            var count = tanks.Length;
            var existingGroups = new HashSet<int>();
            for (var i = 0; i < count; i++)
            {
                var view = tanks[i];
                var entity = _unitsFactory.Build(view);

                var groupId = view.GroupId;
                _groups.Set(entity, new() { GroupId = groupId, UnitIndex = view.UnitIndex });
                existingGroups.Add(groupId);
            }

            foreach (var groupId in existingGroups)
            {
                //_groupBuilder.BuildGroup(groupId);
            }

            existingGroups.Clear();
        }

        public void Dispose() { }
    }
}