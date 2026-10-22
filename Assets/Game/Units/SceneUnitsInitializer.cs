using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs {
    public sealed class SceneUnitsInitializer : IInitializable 
    {
        private readonly UnitsFactory _unitsFactory;
        private readonly SpawnerFactory _spawnerFactory;
        private readonly StringDataDictionary _stringDataDictionary;

        [Inject]
        public SceneUnitsInitializer(
            UnitsFactory unitsFactory,
            SpawnerFactory spawnerFactory,
            StringDataDictionary stringDataDictionary)
        {
            _unitsFactory = unitsFactory;
            _spawnerFactory = spawnerFactory;
            _stringDataDictionary = stringDataDictionary;
        }

        public void Initialize()
        {
            var tanks = GameObject.FindObjectsByType<TankView>(FindObjectsSortMode.None);
            foreach (var tankView in tanks)
            {
                _unitsFactory.Build(tankView);
            }

            var sceneSpawners = GameObject.FindObjectsByType<UnitsSpawnerMono>(FindObjectsSortMode.None);
            foreach (var spawner in sceneSpawners)
            {
                if (!spawner.RegisterAsVirtualPoint)
                    continue;

                _spawnerFactory.CreateSpawnerEntity(spawner);
            }
        }        
    }
}