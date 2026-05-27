using ZE.MechBattle.Navigation;
using ZE.MechBattle.Ecs;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class PortalExitFlowMap : FlowMap, ICalculationSystemPath
    {
        public int Id => _id;
        public readonly NavigationPortalExit PortalExit;

        private readonly int _id;
        

        public PortalExitFlowMap(int id, NavigationPortalExit exit, in FlattenedHexCoordsConverter converter, int length) : base(exit.HexCoord, converter, length)
        {
            _id = id;
            PortalExit = exit;
        }
    }
}
