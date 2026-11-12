using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace ZE.MechBattle.Views
{
    public class ViewProviderFactory : IDisposable
    {
        private object _lockObject = new ();
        private readonly AssetsManager _assetsManager;
        private readonly StringDataDictionary _stringsDict;
        private readonly Dictionary<int, IViewProvider> _providers = new();

        [Inject]
        public ViewProviderFactory(AssetsManager assetsManager, StringDataDictionary stringDataDictionary)
        {
            _assetsManager = assetsManager;
            _stringsDict = stringDataDictionary;
        }

        public IViewProvider GetViewProvider(int id)
        {
            if (_providers.TryGetValue(id, out var provider))
            {
                return provider;
            }

            if (!_stringsDict.TryGetStringByKey(id, out var strKey))
            {
                Debug.LogError("view key not registered: " + strKey);
                return null;
            }
            var assetKey = strKey;
            //

            var assetProvider = _assetsManager.GetAssetProvider(assetKey) as IMonoAssetProvider;
            if (assetProvider.IsReadyToProvideProperty.CurrentValue)
            {
                OnViewLoaded(assetProvider, id);
            }
            else
            {
                lock(_lockObject)
                {
                    var cachedId = id;
                    _providers.Add(cachedId, new AwaitingViewProvider(assetProvider, provider => OnViewLoaded(provider, cachedId)));
                }
            }
            return _providers[id];
        }    

        public void Dispose()
        {
            foreach (var provider in _providers.Values)
            {
                provider.Dispose();
            }
            _providers.Clear();
        }

        private void OnViewLoaded(IMonoAssetProvider assetProvider, int id)
        {
            var view = assetProvider.GetComponent<SimpleView>();
            if (view is PoolableMonoView poolableView)
                AddViewPool(poolableView, id);
            else
                AddInstanceProvider(view, id);
        }

        private void AddViewPool(PoolableMonoView asset, int id)
        {
            lock (_lockObject)
            {
                if (_providers.TryGetValue(id, out var placeholder))
                    placeholder.Dispose();
                var pool = new ViewsPool(_stringsDict.GetStringByKey(id), asset, null) ;            
                _providers[id] = pool;
            }            
        }

        private void AddInstanceProvider(SimpleView view, int id)
        {
            lock (_lockObject)
            {
                if (_providers.TryGetValue(id, out var placeholder))
                    placeholder.Dispose();       
                _providers[id] = new ViewInstanceProvider(view);
            }  
        }
    }
}
