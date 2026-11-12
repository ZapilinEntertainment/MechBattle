using System.Collections.Generic;
using VContainer;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class ViewApplicationStrategiesList
    {
        public IViewApplyStrategy this[ViewMode viewType] => _strategies[viewType];
        private readonly Dictionary<ViewMode, IViewApplyStrategy> _strategies;

        [Inject]
        public ViewApplicationStrategiesList(IEnumerable<IViewApplyStrategy> strategies)
        {
            foreach (var strategy in strategies)
            {
                _strategies.Add(strategy.RealizingViewType, strategy);
            }
        }

    }
}
