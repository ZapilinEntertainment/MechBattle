using UnityEngine;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;
using ZE.MechBattle;

public abstract class FeaturedScopeBase<FeatureInstallerType, FeatureInitializerType, FeaturePostInitializerType> : LifetimeScope
        where FeatureInstallerType : IFeatureInstaller
        where FeatureInitializerType: IFeatureInitializer
        where FeaturePostInitializerType: IFeaturePostInitializer
{
    [SerializeField] private ScopeConfigurationComponent[] _autoRegisterMonoBehaviours;
    protected IReadOnlyList<IFeatureModule> FeatureModulesList { get;private set; }   

    protected override void Configure(IContainerBuilder builder)
    {
        if (_autoRegisterMonoBehaviours != null)
        {
            foreach (var configData in _autoRegisterMonoBehaviours)
                configData.OnConfigure(builder);
        }
       

        FeatureModulesList = Parent.Container.Resolve<FeaturesModulesList>().Modules;
        BindInstallers(builder);
        builder.RegisterBuildCallback(container => 
        { 
            InitializeFeatures();
            PostInitializeFeatures();
        }
        );
    }


    private void BindInstallers(IContainerBuilder builder)
    {
        foreach (var featureModule in FeatureModulesList)
        {
            if (featureModule is FeatureInstallerType installer)
                Install(installer, builder);
        }
    }

    private void InitializeFeatures()
    {
        foreach (var featureModule in FeatureModulesList)
        {
            if (featureModule is FeatureInitializerType initializer)
                FeatureInitialize(initializer, Container);
        }
    }

    private void PostInitializeFeatures()
    {
        foreach (var featureModule in FeatureModulesList)
        {
            if (featureModule is FeaturePostInitializerType postInitializer)
                FeaturePostInitialize(postInitializer, Container);
        }
    }

    protected abstract void Install(FeatureInstallerType installer, IContainerBuilder containerBuilder);
    protected abstract void FeatureInitialize(FeatureInitializerType initializer, IObjectResolver resolver);
    protected abstract void FeaturePostInitialize(FeaturePostInitializerType postInitializer, IObjectResolver resolver);
}
