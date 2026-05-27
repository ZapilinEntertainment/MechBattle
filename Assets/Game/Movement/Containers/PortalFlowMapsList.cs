using System.Collections.Generic;
using UnityEngine;
using ZE.Utils;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Navigation
{

    public class PortalFlowMapsList : UseTimeStoringDictionary<int, PortalExitFlowMap>, IPathStorage<PortalExitFlowMap>
    {
        public bool TryGetValue(int pathId, out PortalExitFlowMap path) => TryGetValue(pathId, out path, updateUsingTime: true);
    }
}
