using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class NavigationGridHandler
    {
        private readonly IMovementCellsMap _movementCellsList;
        private readonly IHexPortalsCoordinator _portalsCoordinator;
        private readonly Stash<HexCoordComponent> _hexCoords;

        [Inject]
        public NavigationGridHandler(IMovementCellsMap movementCellsList, IHexPortalsCoordinator portalsCoordinator, World world)
        {
            _movementCellsList = movementCellsList;
            _portalsCoordinator = portalsCoordinator;
            _hexCoords = world.GetStash<HexCoordComponent>();
        }

        public int2 GetTargetHexCoord(Entity navigationAgentEntity, int portalId)
        {
            var entityHexCoord = _hexCoords.Get(navigationAgentEntity).Value;
            var portal = _portalsCoordinator.GetPortal(portalId);
            if (math.all(portal.HexCoordA == entityHexCoord))
                return portal.HexCoordB;
            else
                return portal.HexCoordA;
        }

        public bool IsCellOccupied(IntTriangularPos pos) => _movementCellsList.TryGetValue(pos, out var cellValue) && cellValue.ProjectionStepIndex == 0;

    }
}
