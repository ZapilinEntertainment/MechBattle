using VContainer;

public interface IFeatureInitializer { }

public interface ISessionFeatureInitializer : IFeatureInitializer
{
    void OnSessionContainerBuilt(IObjectResolver resolver);
}

public interface ISceneFeatureInitializer : IFeatureInitializer
{
    void OnSceneContainerBuilt(IObjectResolver resolver);
}
