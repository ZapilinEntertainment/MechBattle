using VContainer;

public interface IFeatureInstaller
{
}

public interface IAppFeatureScopeInstaller : IFeatureInstaller
{
    void AppScopeInstall(IContainerBuilder builder);
}

public interface ISessionFeatureScopeInstaller : IFeatureInstaller
{
    void SessionScopeInstall(IContainerBuilder builder);
}
public interface ISceneFeatureScopeInstaller : IFeatureInstaller
{
    void SceneScopeInstall(IContainerBuilder builder);
}
