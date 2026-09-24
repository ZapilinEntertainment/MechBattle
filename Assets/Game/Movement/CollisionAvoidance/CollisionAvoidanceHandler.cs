using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class CollisionAvoidanceHandler
    {
        private readonly IMovementDensityMap _densityMap;
        private readonly AffinityHandler _affinityHandler;

        [Inject]
        public CollisionAvoidanceHandler(IMovementDensityMap densityMap, AffinityHandler affinityHandler)
        {
            _densityMap = densityMap;
            _affinityHandler = affinityHandler;
        }

        public int CorrectFlowMapDirection(int2 hexCoord, IntTriangularPos tripos, int originalDirection)
        {
            var bestOption = GetBestNextTriangleOptionCommand.Execute(tripos, _densityMap, originalDirection);
            return bestOption.Direction;
        }

        public bool TryGetDetour(IntTriangularPos currentTripos, IntTriangularPos nextTripos, out IntTriangularPos detourTripos)
        {
            var direction = currentTripos.IsPeak
                ? (int)TriangularMath.DefinePeakNeighbour(currentTripos, nextTripos)
                : (int)TriangularMath.DefineValleyNeighbour(currentTripos, nextTripos);

            var bestOption = GetBestNextTriangleOptionCommand.Execute(currentTripos, _densityMap, direction);
            if (bestOption.Direction != direction)
            {
                detourTripos = bestOption.Tripos;
                //UnityEngine.Debug.Log($"detour found: {currentTripos} -> {detourTripos}");
                return true;
            }
            else
            {
                //UnityEngine.Debug.Log($"best option not found at {currentTripos}: {bestOption.Direction}");
                detourTripos = currentTripos;
                return false;
            }
        }

        public bool CanEntitiesGoThrough(Entity entityA, Entity entityB)
        {
            return !_affinityHandler.AreEntitiesHostile(entityA, entityB);
            //var agentA = _navigationAgents.Get(entityA);
            //var agentB = _navigationAgents.Get(entityB);
            
        }
    
    }
}
