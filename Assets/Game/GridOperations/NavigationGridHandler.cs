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
        private CellResultsListOperator _cellResultsList;

        private readonly IMovementCellsMap _movementCellsList;
        private readonly IHexPortalsCoordinator _portalsCoordinator;
        private readonly IEntitiesNavigationMap _navigationMap;

        private readonly Stash<HexCoordComponent> _hexCoords;
        private readonly Stash<CellPassabilityBlockComponent> _blockComponents;
        private readonly Stash<CellMovementDataComponent> _cellMovementData;
       

        [Inject]
        public NavigationGridHandler(
            IMovementCellsMap movementCellsList, 
            IHexPortalsCoordinator portalsCoordinator, 
            IEntitiesNavigationMap navigationMap,
            World world)
        {
            _movementCellsList = movementCellsList;
            _portalsCoordinator = portalsCoordinator;
            _navigationMap = navigationMap;

            _cellResultsList = new(32);

            _hexCoords = world.GetStash<HexCoordComponent>();
            _blockComponents = world.GetStash<CellPassabilityBlockComponent>();
            _cellMovementData = world.GetStash<CellMovementDataComponent>();
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


        public void BlockCellsPassability(Entity blockingEntity, BlockZoneConfigComponent blockZoneData, RigidTransform point)
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

            foreach (var tripos in _cellResultsList.List)
            {
                if (!_navigationMap.TryGetNavigationCellEntity(tripos, out var cellEntity))
                    continue;

                SetNavigationCellBlock(cellEntity, blockingEntity, blockZoneData.BlockFlags);
            }
            _cellResultsList.Clear();
        }

        private void SetNavigationCellBlock(Entity cellEntity, Entity blockingEntity, CellOccupationLevelFlags occupationFlags)
        {
            ref var existingBlockComponent = ref _blockComponents.Get(cellEntity, out var blockAlreadyExists);
            if (blockAlreadyExists)
            {
                existingBlockComponent.OccupationFlags |= occupationFlags;
                existingBlockComponent.SecondaryOccupationEntity = blockingEntity;
            }
            else
            {
                _blockComponents.Add(cellEntity, new(blockingEntity, occupationFlags));
            }
        }
    }
}
