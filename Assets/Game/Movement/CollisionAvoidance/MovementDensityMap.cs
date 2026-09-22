using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Movement.CollisionAvoidance
{
    public class MovementDensityMap : IMovementDensityMap
    {
        private readonly IEntitiesNavigationMap _map;
        private readonly Stash<CellMovementDensityComponent> _cellMovementDensity;

        [Inject]
        public MovementDensityMap(IEntitiesNavigationMap entitiesNavigationMap, World world)
        {
            _map = entitiesNavigationMap;

            _cellMovementDensity = world.GetStash<CellMovementDensityComponent>();
        }


        public bool TryGetDensity(IntTriangularPos tripos, out float density)
        {
            density = 0f;
            if (!_map.TryGetEntity(tripos, out var cellEntity))
                return false;

            density = GetComponentOrDefaultValueCommand.Execute(cellEntity, _cellMovementDensity, 0f);
            return density != 0f;
        }
    }
}
