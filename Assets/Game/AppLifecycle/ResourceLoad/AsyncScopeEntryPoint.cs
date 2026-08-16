using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public abstract class AsyncScopeEntryPoint<ResourceLoaderType> : IAsyncStartable
       where ResourceLoaderType : IAsyncScopeResourceLoader
{
    private readonly FeaturesModulesList _modulesList;


    [Inject]
    public AsyncScopeEntryPoint(FeaturesModulesList modulesList)
    {
        _modulesList = modulesList;
    }

    public abstract Awaitable StartAsync(CancellationToken cancellation = default);

    protected Awaitable<IResourceBinder> LoadResourcesAsync(CancellationToken cancellation) =>
        LoadFeatureResourcesParallelCommand.Execute<ResourceLoaderType>(_modulesList, cancellation, LoadResourcesAsync);

    abstract protected Awaitable<IResourceBinder> LoadResourcesAsync(ResourceLoaderType resourceLoader);
}
