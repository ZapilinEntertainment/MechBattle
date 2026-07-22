using UnityEngine;
using VContainer;

public interface IAsyncScopeResourceLoader
{
    //Awaitable<IResourceBinder> LoadResourcesAsync();
    //basic method not possible - feature modules can contain both interface realizations
}

public interface ISessionAsyncResourceLoader : IAsyncScopeResourceLoader
{
    Awaitable<IResourceBinder> LoadSessionResourcesAsync(IObjectResolver resolver);
}
public interface ISceneAsyncResourceLoader : IAsyncScopeResourceLoader
{
    Awaitable<IResourceBinder> LoadSceneResourcesAsync(IObjectResolver resolver);
}
