using System.Collections.Generic;
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
        private readonly INavigationMap _navigationMap;
        private readonly Stash<HexCoordComponent> _hexCoords;
        private readonly CellBlockTokensList _cellBlockTokensList;

        private int _nextBlockingId = 1;
        private List<IntTriangularPos> _blockList = new(16);

        [Inject]
        public NavigationGridHandler(
            IMovementCellsMap movementCellsList, 
            IHexPortalsCoordinator portalsCoordinator, 
            INavigationMap navigationMap,
            World world,
            CellBlockTokensList blockTokensList)
        {
            _movementCellsList = movementCellsList;
            _portalsCoordinator = portalsCoordinator;
            _navigationMap = navigationMap;
            _cellBlockTokensList = blockTokensList;

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

        public bool IsCellObstructed(IntTriangularPos pos) => !_navigationMap.GetPassabilityData(pos).IsPassable;

        public int BlockCellsPassability(BlockZoneConfigComponent blockZoneData, RigidTransform point)
        {
            // note: GetStepAffectedTrianglesJob
            //switch (blockZoneData.Mode)
            //{
            //    case BlockZoneMode.Hex:
            //        {
            //            GetTrianglesInRadiusJob
            //            break;
            //        }
            //}

            return -1;
        }
    }
}
