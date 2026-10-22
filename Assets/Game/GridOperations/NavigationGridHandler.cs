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
        private int _nextBlockingId = 1;
        private CellResultsListOperator _cellResultsList;

        private readonly IMovementCellsMap _movementCellsList;
        private readonly IHexPortalsCoordinator _portalsCoordinator;
        private readonly IEntitiesNavigationMap _navigationMap;
        private readonly CellBlockTokensList _cellBlockTokensList;

        private readonly Stash<HexCoordComponent> _hexCoords;
        private readonly Stash<CellPassabilityBlockComponent> _blockComponents;       

       

        [Inject]
        public NavigationGridHandler(
            IMovementCellsMap movementCellsList, 
            IHexPortalsCoordinator portalsCoordinator, 
            IEntitiesNavigationMap navigationMap,
            World world,
            CellBlockTokensList blockTokensList)
        {
            _movementCellsList = movementCellsList;
            _portalsCoordinator = portalsCoordinator;
            _navigationMap = navigationMap;
            _cellBlockTokensList = blockTokensList;

            _cellResultsList = new(32);

            _hexCoords = world.GetStash<HexCoordComponent>();
            _blockComponents = world.GetStash<CellPassabilityBlockComponent>();
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
            switch(blockZoneData.Mode)
            {
                case BlockZoneMode.Round:
                    {
                        AffectedTrianglesCalculator.CalculateRoundAffectedTrianglesManaged(_cellResultsList, point.pos, blockZoneData.Value, _navigationMap.TriangleHeight);
                        break;
                    }
                case BlockZoneMode.Rect:
                    {
                        AffectedTrianglesCalculator.CalculateRectAffectedTrianglesManaged(_cellResultsList, point, blockZoneData.DecodeSize2D(), _navigationMap.TriangleHeight);
                        break;
                    }
                default:
                    {
                        throw new System.NotImplementedException($"{blockZoneData.Mode} not implemented");
                    }
            }

            var blockTokenId = _nextBlockingId++;
            foreach (var tripos in _cellResultsList.List)
            {
                if (!_navigationMap.TryGetNavigationCellEntity(tripos, out var cellEntity))
                    continue;

                SetNavigationCellBlock(cellEntity, blockTokenId, blockZoneData.BlockFlags);
            }
            _cellResultsList.Clear();

            return blockTokenId;
        }

        private void SetNavigationCellBlock(Entity cellEntity, int tokenId, CellOccupationLevelFlags occupationFlags)
        {
            ref var existingBlockComponent = ref _blockComponents.Get(cellEntity, out var blockAlreadyExists);
            if (blockAlreadyExists)
            {
                existingBlockComponent.OccupationFlags |= occupationFlags;
                existingBlockComponent.SecondaryOccupationTokenId = tokenId;
            }
            else
            {
                _blockComponents.Add(cellEntity, new(tokenId, occupationFlags));
            }
        }
    }
}
