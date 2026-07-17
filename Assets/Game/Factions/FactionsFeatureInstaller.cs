using UnityEngine;
using VContainer;

namespace ZE.MechBattle
{
    public class FactionsFeatureInstaller : IFeatureInstaller
    {
        public void InstallDependencies(IContainerBuilder builder)
        {
            builder.Register<FinalViewFunctionalApplier>(Lifetime.Scoped);
            builder.Register<PlayerRelations>(Lifetime.Scoped);
            builder.Register<IPlayersList, PlayersList>(Lifetime.Scoped);
        }

        public void Initialize(IObjectResolver resolver) { }

        

        public void PostInitialize(IObjectResolver resolver) { }

        public void PreloadResources(IObjectResolver globalContainerResolver) { }
    }
}
