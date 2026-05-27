using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class HexPortalsPath : PathData<PortalPathDestinationKey, int>, ICalculationSystemPath
    {
        int ICalculationSystemPath.Id => Id;

        public HexPortalsPath(int id, (PortalPathDestinationKey, PortalPathDestinationKey) destinationKey) : base(id, destinationKey)
        {
        }

        
    }
}
