using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class CollisionAvoidanceHandler
    {
        private readonly IMovementDensityMap _densityMap;

        [Inject]
        public CollisionAvoidanceHandler(IMovementDensityMap densityMap)
        {
            _densityMap = densityMap;
        }

        public int CorrectFlowMapDirection(int2 hexCoord, IntTriangularPos tripos, FlowMap flowMap, int originalDirection)
        {
            var bestOption = GetBestNextTriangleOptionCommand.Execute(tripos, flowMap, _densityMap, originalDirection);
            return bestOption.Direction;
        }
    
    }
}
