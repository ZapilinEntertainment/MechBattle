using VContainer;

public interface IFeaturePostInitializer { }
public interface ISessionFeaturePostInitializer : IFeaturePostInitializer
{
    void OnSessionContainerPostBuilt(IObjectResolver resolver);
}
public interface ISceneFeaturePostInitializer : IFeaturePostInitializer
{
    void OnSceneContainerPostBuilt(IObjectResolver resolver);
}
