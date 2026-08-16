using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public delegate Awaitable<IResourceBinder> LoadBinderDelegate<ResourceLoaderType>(ResourceLoaderType loader);
public delegate Awaitable<ResourceType> LoadResourceDelegate<ResourceType, ResouceLoaderType>(ResouceLoaderType loader);

public static class LoadFeatureResourcesParallelCommand
{
    public static async Awaitable<IResourceBinder> Execute<ResourceLoaderType>(
    FeaturesModulesList featureModules,
    CancellationToken cancellation,
    LoadBinderDelegate<ResourceLoaderType> AsyncLoadFunc)
    {
        //var taskList = new List<Awaitable<IResourceBinder>>();
        //foreach (var featureModule in featureModules.Modules)
        //{
        //    if (featureModule is ResourceLoaderType featureLoader)
        //    {
        //        var task = AsyncLoadFunc(featureLoader);
        //        if (task != null)
        //            taskList.Add(task);
        //    }
        //}
        var taskList = PrepareTasksList<ResourceLoaderType, IResourceBinder>(featureModules, cancellation, (loader) => AsyncLoadFunc(loader));

        if (taskList.Count == 1)
            return await taskList[0];

        var results = await AwaitablesExtensions.WhenAll(taskList);
        if (cancellation.IsCancellationRequested || results == null)
            return new AsyncResourcesScopeBinder();

        var binder = new AsyncResourcesScopeBinder(results);
        return binder;
    }

    // simple load parallel
    public static async Awaitable<IReadOnlyList<ResourceType>> Execute<ResourceLoaderType, ResourceType>(
    FeaturesModulesList featureModules,
    CancellationToken cancellation,
    LoadResourceDelegate<ResourceType, ResourceLoaderType> AsyncLoadFunc)
    {
        var taskList = PrepareTasksList<ResourceLoaderType, ResourceType>(featureModules, cancellation, AsyncLoadFunc);

        if (taskList.Count == 1)
        {
            var result = await taskList[0];
            return new ResourceType[1] { result};
        }
            
        else
        {
            return await AwaitablesExtensions.WhenAll(taskList);
        }            
    }

    private static List<Awaitable<ResourceType>> PrepareTasksList<ResourceLoaderType, ResourceType>(
     FeaturesModulesList featureModules,
     CancellationToken cancellation,
     LoadResourceDelegate<ResourceType, ResourceLoaderType> AsyncLoadFunc)
    {
        var taskList = new List<Awaitable<ResourceType>>();
        foreach (var featureModule in featureModules.Modules)
        {
            if (featureModule is ResourceLoaderType featureLoader)
            {
                var task = AsyncLoadFunc(featureLoader);
                if (task != null)
                    taskList.Add(task);
            }
        }
        return taskList;
    }
}
