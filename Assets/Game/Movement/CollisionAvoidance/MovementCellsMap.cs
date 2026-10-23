using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Movement.CollisionAvoidance
{
    public class MovementCellsMap : IMovementCellsMap
    {
        private Stash<CellMovementDataComponent> _cellMovementDatas;
        private Stash<MovementCollisionAvoidanceComponent> _avoidances;
        private Stash<CellPassabilityBlockComponent> _cellBlocks;
        private readonly IEntitiesNavigationMap _entitiesMap;

        [Inject]
        public MovementCellsMap(World world, IEntitiesNavigationMap entitiesNavigationMap)
        {
            _entitiesMap = entitiesNavigationMap;

            _cellMovementDatas = world.GetStash<CellMovementDataComponent>();
            _avoidances = world.GetStash<MovementCollisionAvoidanceComponent>();
            _cellBlocks = world.GetStash<CellPassabilityBlockComponent>(); 
        }

        public bool TryGetValue(IntTriangularPos tripos, out CellMovementData cellData)
        {
            if (_entitiesMap.TryGetEntity(tripos, out var entity) && TryGetCellData(entity, out cellData))
                return true;

            cellData = default;
            return false;
        }

        public bool TryWriteCell(IntTriangularPos tripos, CellMovementData cellData)
        {
            if (!_entitiesMap.TryGetEntity(tripos, out var cellEntity))
                return false;

            if (TryGetCellData(cellEntity, out var existingCellData) && existingCellData > cellData)
                return false;

            SetCellData(cellEntity, cellData);
            return true;
        }

        private bool TryGetCellData(Entity entity, out CellMovementData cellData)
        {
            var component = _cellMovementDatas.Get(entity, out var exists);
            if (exists)
            {
                cellData = component.Value;
                return true;
            }
            else
            {
                cellData = default;
                return false;
            }
        }

        private void SetCellData(Entity entity, CellMovementData cellData) =>
            _cellMovementDatas.Set(entity, new(cellData));

        public bool TryWriteCell(IntTriangularPos tripos, Entity entity, float2 moveDir, int projectionIndex, CellOccupationLevelFlags occupationLevelFlags)
        {
            var avoidance = _avoidances.Get(entity, out var exists);
            var cellData = new CellMovementData(entity, exists ? avoidance.Priority : MovementCollisionAvoidancePriority.None, moveDir, projectionIndex, occupationLevelFlags);
            return TryWriteCell(tripos, cellData);
        }

        public void Clear() => _cellMovementDatas.RemoveAll();

        public void ApplyCellBlock(Entity cellEntity, Entity blockerEntity)
        {
            var blockComponent = _cellBlocks.Get(cellEntity, out var exists);
            if (!exists)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"you trying to block cell {cellEntity.GetComponent<CellEntityComponent>().Tripos} by entity {blockerEntity.Id} with no block component");
#endif
                return;
            }

            var occupationFlags = blockComponent.OccupationFlags;
            if (TryGetCellData(cellEntity, out var existingData))
                occupationFlags |= existingData.OccupationFlags;

            SetCellData(cellEntity, new(blockerEntity, MovementCollisionAvoidancePriority.Immovable, float2.zero, 0, occupationFlags));
        }
    }
}