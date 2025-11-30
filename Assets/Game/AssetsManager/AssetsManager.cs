using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ZE.MechBattle.AssetsManagement;
using R3;

namespace ZE.MechBattle
{
    public interface IAssetProvider : IDisposable
    {
        T GetValue<T>() where T : MonoBehaviour;
        ReadOnlyReactiveProperty<bool> IsReadyToProvideProperty { get; }
    }

    public class AssetsManager : IDisposable
    {
        private readonly Dictionary<string, IAssetProvider> _cachedAssets = new();


        public IAssetProvider GetAssetProvider(string assetKey)
        {
            if (!_cachedAssets.TryGetValue(assetKey, out var cachedAsset))
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(assetKey);
                cachedAsset = new AssetProvider(handle);
                _cachedAssets.Add(assetKey, cachedAsset);
            }

            return cachedAsset;
        }

        public void Dispose()
        {
            foreach (var cachedAsset in _cachedAssets.Values)
            {
                cachedAsset.Dispose();
            }
            _cachedAssets.Clear();
        }
    }
}
