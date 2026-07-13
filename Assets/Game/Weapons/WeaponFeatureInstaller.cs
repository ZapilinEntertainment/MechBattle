using UnityEngine;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class WeaponFeatureInstaller : IFeatureInstaller
    {
        private WeaponSystemsInstallQueue _systemsQueue = new();

        public void PreloadResources(IObjectResolver globalContainerResolver)
        {

        }

        public void InstallDependencies(IContainerBuilder builder)
        {
            builder.Register<WeaponFactory>(Lifetime.Scoped);

            _systemsQueue.InstallDependencies(builder);
        }

        

        public void Initialize(IObjectResolver resolver)
        {
            _systemsQueue.Initialize(resolver);
        }

        public void PostInitialize(IObjectResolver resolver) { }


    }
}
