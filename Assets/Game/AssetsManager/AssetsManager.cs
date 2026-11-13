using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using R3;
using ZE.MechBattle.AssetsManagement;

namespace ZE.MechBattle
{
    public interface IMonoAssetProvider : IAssetProvider
    {
        T GetComponent<T>() where T : MonoBehaviour;
    }

    public interface IAssetProvider : IDisposable
    {
        ReadOnlyReactiveProperty<bool> IsReadyToProvideProperty { get; }
        bool TryGetValue<T>(out T value);
    }

    public class AssetsManager : IDisposable
    {
        public enum AssetRequestResult : byte { AssetNotCached, AssetNotLoaded, Success}

        private readonly Dictionary<string, IAssetProvider> _cachedAssets = new();

        public AssetRequestResult TryGetValue<T>(string key, out T value)
        {
            if (!_cachedAssets.TryGetValue(key, out var assetProvider))
            {
                value = default;
                return AssetRequestResult.AssetNotCached;
            }

            if (!assetProvider.TryGetValue(out value))
                return AssetRequestResult.Success;

            return AssetRequestResult.AssetNotLoaded;
        }

        public async Awaitable LoadAndCacheAssetsByLabel<T>(params string[] labels) where T : MonoBehaviour
        {
            var locations = await Addressables.LoadResourceLocationsAsync(labels, Addressables.MergeMode.Union, typeof(T));
            var handles = new List<AsyncOperationHandle>();
            foreach (var location in locations)
            {
                var handle = Addressables.LoadAssetAsync<T>(location);
                var key = location.PrimaryKey;
                if (_cachedAssets.ContainsKey(key))
                    continue;

                var assetProvider = new AssetProvider<T>(handle);
                _cachedAssets.Add(key, assetProvider);
                handles.Add(handle);
            }

            var allHandlesReady = false;
            do
            {
                foreach (var handle in handles)
                {
                    if (!handle.IsDone)
                    {
                        allHandlesReady = false;
                        break;
                    }
                }
                await Awaitable.NextFrameAsync();
            }
            while (!allHandlesReady);
            Addressables.Release(locations);
        }

        public IMonoAssetProvider GetMonoAssetProvider(string assetKey)
        {
            IMonoAssetProvider provider;
            if (!_cachedAssets.TryGetValue(assetKey, out var cachedAsset))
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(assetKey);
                provider = new GameObjectAssetProvider(handle);
                _cachedAssets.Add(assetKey, provider);
            }
            else
            {
                provider = cachedAsset as IMonoAssetProvider;
            }

            return provider;
        }

        public void Dispose()
        {
            foreach (var cachedAsset in _cachedAssets.Values)
            {
                cachedAsset.Dispose();
            }
            _cachedAssets.Clear();
        }

        public static async Awaitable LoadAssetsByLabel<T>(Action<T, string> handleAction, params string[] labels)
        {
            var locations = await Addressables.LoadResourceLocationsAsync(labels, Addressables.MergeMode.Union, typeof(T));
            foreach (var location in locations)
            {
                var result = await Addressables.LoadAssetAsync<T>(location);
                handleAction(result, location.PrimaryKey);
            }
            Addressables.Release(locations);
        }

        public static async Awaitable<T> LoadAssetDirectly<T>(string assetKey) 
        {
            var handle = Addressables.LoadAssetAsync<T>(assetKey);
            while (!handle.IsDone)
                await Awaitable.NextFrameAsync();

            return handle.Result;
        }

        public static async Awaitable<T> LoadComponentAssetDirectly<T>(string assetKey) where T : MonoBehaviour
        {
            var go = await LoadAssetDirectly<GameObject>(assetKey);
            return go.GetComponent<T>();
        }
    }
}
