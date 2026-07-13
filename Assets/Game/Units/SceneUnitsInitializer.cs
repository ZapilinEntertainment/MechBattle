using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs {
    public sealed class SceneUnitsInitializer : IInitializable 
    {
        private readonly UnitsFactory _unitsFactory;
        private readonly StringDataDictionary _stringDataDictionary;
        private readonly ISpawnersManager _spawnersManager;

        [Inject]
        public SceneUnitsInitializer(UnitsFactory unitsFactory, ISpawnersManager spawnersManager, StringDataDictionary stringDataDictionary)
        {
            _unitsFactory = unitsFactory;
            _spawnersManager = spawnersManager;
            _stringDataDictionary = stringDataDictionary;
        }

        public void Initialize()
        {
            var tanks = GameObject.FindObjectsByType<TankView>(FindObjectsSortMode.None);
            foreach (var tankView in tanks)
            {
                _unitsFactory.Build(tankView);
            }

            var sceneSpawners = GameObject.FindObjectsByType<UnitsSpawner>(FindObjectsSortMode.None);
            foreach (var spawner in sceneSpawners)
            {
                spawner.Inject(_stringDataDictionary);
                _spawnersManager.Register(spawner);
            }
        }        
    }
}