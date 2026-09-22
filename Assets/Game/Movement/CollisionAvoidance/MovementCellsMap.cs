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
        private readonly IEntitiesNavigationMap _entitiesMap;

        [Inject]
        public MovementCellsMap(World world, IEntitiesNavigationMap entitiesNavigationMap)
        {
            _entitiesMap = entitiesNavigationMap;

            _cellMovementDatas = world.GetStash<CellMovementDataComponent>();
            _avoidances = world.GetStash<MovementCollisionAvoidanceComponent>();
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

        public bool TryWriteCell(IntTriangularPos tripos, Entity entity, float2 moveDir, int projectionIndex)
        {
            var avoidance = _avoidances.Get(entity, out var exists);
            var cellData = new CellMovementData(entity, exists ? avoidance.Priority : MovementCollisionAvoidancePriority.None, moveDir, projectionIndex);
            return TryWriteCell(tripos, cellData);
        }

        public void Clear() => _cellMovementDatas.RemoveAll();
    }
}