using Scellecs.Morpeh;
using System.Collections.Generic;
using VContainer;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class MonoViewFactory
    {
        private readonly Dictionary<string, IViewInstanceStrategy> _viewInstanceStrategies = new();
        private readonly IViewInstanceStrategy _delayedMonoInstanceStrategy;
        private readonly IViewInstanceStrategy _virtualViewInstanceStrategy;

        [Inject]
        public MonoViewFactory(
            VirtualViewFactory virtualViewFactory,
            DelayedMonoViewFactory delayedMonoViewFactory)
        {
            _delayedMonoInstanceStrategy = delayedMonoViewFactory;
            _virtualViewInstanceStrategy = virtualViewFactory;
        }

        // can be mono-object or virtual
        public Entity CreateViewEntity(string viewId)
        {
            if (!_viewInstanceStrategies.TryGetValue(viewId, out var strategy))
            {
                if (viewId.StartsWith(ViewConstants.VIRTUAL_VIEW_PREFIX, System.StringComparison.Ordinal))
                {
                    strategy = _virtualViewInstanceStrategy;
                }
                else
                {
                    strategy = _delayedMonoInstanceStrategy;
                }
                _viewInstanceStrategies.Add(viewId, strategy);
            }

            return strategy.CreateViewEntity(viewId);
        }
        
        
    }
}
