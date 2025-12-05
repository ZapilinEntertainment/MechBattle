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

        [Inject]
        public SceneUnitsInitializer(UnitsFactory unitsFactory)
        {
            _unitsFactory = unitsFactory;
        }

        public void OnAwake() 
        {
            var tanks = GameObject.FindObjectsByType<TankView>(FindObjectsSortMode.None);
            foreach (var tankView in tanks)
            {
                _unitsFactory.Build(tankView);
            }
        }

        public void Dispose() { }
    }
}