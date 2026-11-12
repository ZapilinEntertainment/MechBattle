using System.Collections.Generic;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Views
{
    public class CompositeVirtualViewConfigsList
    {
        private readonly Dictionary<string, VirtualViewComponent[]> _compositeData = new();
        private readonly VirtualViewConfigsList _configsList;

        [Inject]
        public CompositeVirtualViewConfigsList(VirtualViewConfigsList configsList)
        {
            _configsList = configsList;
        }

        public bool TryGetCompositeVirtualViewData(string key, out VirtualViewComponent[] components)
        {
            if (!_compositeData.TryGetValue(key, out components))
            {
                if (!_configsList.TryGetElement(key, out var config))
                {
                    UnityEngine.Debug.LogError(key + " view config not found");
                    return false;
                }

               
            }

            throw new System.NotImplementedException();
        }
    
    }
}
