using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class CollisionAvoidanceHandler
    {
        private readonly MovementDensityMapsManager _densityMapsManager;

        [Inject]
        public CollisionAvoidanceHandler(MovementDensityMapsManager movementDensityMapsManager)
        {
            _densityMapsManager = movementDensityMapsManager;
        }


        public int CorrectFlowMapDirection(int2 hexCoord, IntTriangularPos tripos, FlowMap flowMap, int originalDirection)
        {
            var densityMap = _densityMapsManager.GetDensityMap(hexCoord);
            var bestOption = GetBestNextTriangleOptionCommand.Execute(tripos, flowMap, densityMap, originalDirection);
            return bestOption.Direction;
        }
    
    }
}
