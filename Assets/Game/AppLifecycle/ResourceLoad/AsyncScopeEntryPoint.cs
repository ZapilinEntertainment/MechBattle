using System.Collections.Generic;
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

    protected async Awaitable<IResourceBinder> LoadResourcesAsync(CancellationToken cancellation)
    {
        var taskList = new List<Awaitable<IResourceBinder>>();
        foreach (var featureModule in _modulesList.Modules)
        {
            if (featureModule is ResourceLoaderType featureLoader)
            {
                var task = LoadResourcesAsync(featureLoader);
                taskList.Add(task);
            }
        }

        //if (taskList.Count == 1)
        //   return await taskList[0];

        var results = await AwaitablesExtensions.WhenAll(taskList);
        if (cancellation.IsCancellationRequested || results == null)
            return new AsyncResourcesScopeBinder();

        var binder = new AsyncResourcesScopeBinder(results);
        return binder;
    }

    abstract protected Awaitable<IResourceBinder> LoadResourcesAsync(ResourceLoaderType resourceLoader);
}
