using Scellecs.Morpeh;
using System.Collections.Generic;
using VContainer;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class ViewApplier
    {
        private readonly Dictionary<string, IViewApplyStrategy> _strategiesByKey = new();
        private readonly ViewApplicationStrategiesList _strategiesByType;


        [Inject]
        public ViewApplier(ViewApplicationStrategiesList strategiesList)
        {
            _strategiesByType = strategiesList;
        }

        // can be mono-object or virtual
        public void ApplyView(Entity entity, string viewId)
        {
            if (!_strategiesByKey.TryGetValue(viewId, out var strategy))
            {
                var type = IViewApplyStrategy.ViewIdToStrategyType(viewId);
                strategy = _strategiesByType[type];
                _strategiesByKey.Add(viewId, strategy);
            }

            strategy.ApplyView(entity, viewId);
        }
    }
}
