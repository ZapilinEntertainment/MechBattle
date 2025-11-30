using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using R3;

namespace ZE.MechBattle.Views
{
    public class ViewProviderFactory : IDisposable
    {
        private object _lockObject = new ();
        private readonly AssetsManager _assetsManager;
        private readonly StringDataDictionary _stringsDict;
        private readonly Dictionary<ViewKey, IViewProvider> _providers = new();

        [Inject]
        public ViewProviderFactory(AssetsManager assetsManager, StringDataDictionary stringDataDictionary)
        {
            _assetsManager = assetsManager;
            _stringsDict = stringDataDictionary;
        }

        public IViewProvider GetViewProvider(ViewKey key)
        {
            if (_providers.TryGetValue(key, out var provider))
            {
                return provider;
            }                

            if (!_stringsDict.TryGetStringByKey(key.IdKey, out var strKey))
            {
                Debug.LogError("view key not registered: " + key.IdKey.ToString());
                return null;
            }

            // temp?
            var assetKey = strKey;

            // TODO:
            var assetProvider = _assetsManager.GetAssetProvider(assetKey);
            if (assetProvider.IsReadyToProvideProperty.CurrentValue)
            {
                OnViewLoaded(assetProvider, key);
            }
            else
            {
                lock(_lockObject)
                {
                    _providers.Add(key, new AwaitingViewProvider(assetProvider, provider => OnViewLoaded(provider, key)));
                }
            }
            return _providers[key];
        }    

        public void Dispose()
        {
            foreach (var provider in _providers.Values)
            {
                provider.Dispose();
            }
            _providers.Clear();
        }

        private void OnViewLoaded(IAssetProvider assetProvider, ViewKey key)
        {
            //Debug.Log("on view loaded");
            var view = assetProvider.GetValue<SimpleView>();
            if (view is PoolableView poolableView)
                AddViewPool(poolableView, key);
            else
                AddInstanceProvider(view, key);
        }

        private void AddViewPool(PoolableView asset, ViewKey key)
        {
            lock (_lockObject)
            {
                if (_providers.TryGetValue(key, out var placeholder))
                    placeholder.Dispose();
                var pool = new ViewsPool(asset) ;            
                _providers[key] = pool;
            }            
        }

        private void AddInstanceProvider(SimpleView view, ViewKey key)
        {
            lock (_lockObject)
            {
                if (_providers.TryGetValue(key, out var placeholder))
                    placeholder.Dispose();       
                _providers[key] = new ViewInstanceProvider(view);
            }  
        }
    }
}
