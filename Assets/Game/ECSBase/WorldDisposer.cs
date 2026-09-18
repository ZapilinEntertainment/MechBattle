using Scellecs.Morpeh;
using System;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class WorldDisposer : IDisposable
    {
        private readonly World _world;
        private readonly FeaturesModulesList _featuresModulesList;
        private readonly IObjectResolver _resolver;

        [Inject]
        public WorldDisposer(FeaturesModulesList modulesList, World world, IObjectResolver resolver)
        {
            _featuresModulesList = modulesList;
            _world = world;
            _resolver = resolver;
        }

        public void Dispose()
        {
            foreach (var featureModule in _featuresModulesList.Modules)
            {
                if (featureModule is IEcsFeatureModule ecsFeatureModule)
                    ecsFeatureModule.OnWorldDispose(_world, _resolver);
            }
        }
    }
}
