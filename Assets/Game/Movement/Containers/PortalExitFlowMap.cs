using ZE.MechBattle.Navigation;
using ZE.MechBattle.Ecs;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class PortalExitFlowMap : FlowMap, ICalculationSystemPath
    {
        public int Id => _id;

        private readonly int _id;
        

        public PortalExitFlowMap(int id, int2 hexCoord, in FlattenedHexCoordsConverter converter, int length) : base(hexCoord, converter, length)
        {
            _id = id;
        }
    }
}
