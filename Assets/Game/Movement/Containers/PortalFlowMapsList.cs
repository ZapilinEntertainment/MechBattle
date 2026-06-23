using System.Collections.Generic;
using UnityEngine;
using ZE.Utils;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Navigation
{
    public interface IFlowMapsList : IEnumerable<KeyValuePair<int, PortalExitFlowMap>>, IPathStorage<PortalExitFlowMap> { }

    public class PortalFlowMapsList : UseTimeStoringDictionary<int, PortalExitFlowMap>, IFlowMapsList
    {
        public bool TryGetPathById(int pathId, out PortalExitFlowMap path) => TryGetValue(pathId, out path, updateUsingTime: true);

        IEnumerator<KeyValuePair<int, PortalExitFlowMap>> IEnumerable<KeyValuePair<int, PortalExitFlowMap>>.GetEnumerator() =>
            Values.GetEnumerator();
    }
}
