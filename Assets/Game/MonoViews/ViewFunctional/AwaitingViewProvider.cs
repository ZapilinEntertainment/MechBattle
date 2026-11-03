using System;
using R3;

namespace ZE.MechBattle.Views
{
    public class AwaitingViewProvider : IViewProvider, IDisposable
    {
        public bool IsReadyToProvide => false;
        private bool _isCompleted = false;
        private readonly IDisposable _subscription;
        private readonly IAssetProvider _provider;
        private readonly Action<IAssetProvider> _onViewLoadedAction;

        public IView GetView() => null;

        public AwaitingViewProvider(IAssetProvider assetProvider, Action<IAssetProvider> onViewLoadedAction)
        {
            _provider = assetProvider;
            _onViewLoadedAction = onViewLoadedAction;
            _subscription = assetProvider
                .IsReadyToProvideProperty
                .Where(x => x == true)
                .Take(1)
                .Subscribe(_ => OnProviderReady());
        }

        public void Dispose() 
        {
            if (!_isCompleted)
                _subscription.Dispose();
        }

        private void OnProviderReady()
        {
            _isCompleted = true;
            _onViewLoadedAction.Invoke(_provider);
        }
    }
}
