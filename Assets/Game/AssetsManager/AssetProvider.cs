using R3;
using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ZE.MechBattle.AssetsManagement
{
    // reactive asset handle wrapper
    public class AssetProvider : IAssetProvider
    {
        public ReadOnlyReactiveProperty<bool> IsReadyToProvideProperty => _isReadyToProvideProperty;

        private readonly ReactiveProperty<bool> _isReadyToProvideProperty = new(false);
        private readonly AsyncOperationHandle<GameObject> _handle;

        public T GetValue<T>() where T : MonoBehaviour => _handle.Result.GetComponent<T>();

        public AssetProvider(AsyncOperationHandle<GameObject> handle)
        {
            _handle = handle;

            if (_handle.IsDone)
            {
                _isReadyToProvideProperty.Value = true;
            }
            else
            {                
                _handle.Completed += _ => _isReadyToProvideProperty.Value = true; 
            }

        }

        public void Dispose()
        {
            _handle.Release();
            _isReadyToProvideProperty.Dispose();
        }
    }
}
