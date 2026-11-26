using R3;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ZE.MechBattle.AssetsManagement
{
    // reactive asset handle wrapper
    public class AssetProvider<T> : IAssetProvider
    {
        public ReadOnlyReactiveProperty<bool> IsReadyToProvideProperty => _isReadyToProvideProperty;
        protected readonly AsyncOperationHandle<T> _handle;

        private readonly ReactiveProperty<bool> _isReadyToProvideProperty = new(false);
       

        public AssetProvider(AsyncOperationHandle<T> handle)
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
            if (_handle.IsValid()) 
                _handle.Release();
            _isReadyToProvideProperty.Dispose();
        }

        public bool TryGetValue<T1>(out T1 value)
        {
            if (_isReadyToProvideProperty.Value && _handle.Result is T1 converted)
            {
                value = converted;
                return true;
            }

            value = default;
            return false;
        }
    }
}
