using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class HexPortalPathsLRUBuffer : UserCountDependentLRUPathsBuffer<PortalPathDestinationKey, int, HexPortalsPath>, IPathStorage<HexPortalsPath>
    {

        public bool TryGetPathById(int pathId, out HexPortalsPath path) => TryGetValue(pathId, out path, updateUsingTime: true);

        protected override HexPortalsPath CreateNewPath(int pathId, PortalPathDestinationKey start, PortalPathDestinationKey end) =>
            new(pathId, (start,end));
    }
}
