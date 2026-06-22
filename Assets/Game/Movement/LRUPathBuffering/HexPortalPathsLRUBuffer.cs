using System.Collections.Generic;
using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public interface IPortalPaths : IEnumerable<KeyValuePair<int, HexPortalsPath>>, IPathStorage<HexPortalsPath>
    {
    }

    public class HexPortalPathsLRUBuffer : 
        UserCountDependentLRUPathsBuffer<PortalPathDestinationKey, int, HexPortalsPath>,
        IPortalPaths
    {

        public bool TryGetPathById(int pathId, out HexPortalsPath path) => TryGetValue(pathId, out path, updateUsingTime: true);

        protected override HexPortalsPath CreateNewPath(int pathId, PortalPathDestinationKey start, PortalPathDestinationKey end) =>
            new(pathId, (start,end));

        IEnumerator<KeyValuePair<int, HexPortalsPath>> IEnumerable<KeyValuePair<int, HexPortalsPath>>.GetEnumerator() =>
            Values.GetEnumerator();
    }
}
