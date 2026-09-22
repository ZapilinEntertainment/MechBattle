using Scellecs.Morpeh;
using Unity.Collections;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public static class FormTargetsMapCommand
    {
        // TODO: will not work for non-moving objects (without navigation agents), require new logics

        public static NativeParallelHashMap<IntTriangularPos, Entity> Execute(
            NativeParallelHashMap<IntTriangularPos, Entity> map, 
            Allocator allocator,
            Filter movementCellsFilter,
            Stash<CellMovementDataComponent> moveCellComponents,
            Stash<CellEntityComponent> cellComponents)
        {
            var count = movementCellsFilter.GetLengthSlow();
            if (map.Capacity < count)
            {
                map.Dispose();
                map = new NativeParallelHashMap<IntTriangularPos, Entity>(math.ceilpow2(count), allocator);
            }
            else
            {
                map.Clear();
            }

            foreach (var cellEntity in movementCellsFilter)
            {
                var movementData = moveCellComponents.Get(cellEntity).Value;
                if (!movementData.IsRealOccupationCell)
                    continue;

                var occupiedEntity = movementData.Entity;
                var tripos = cellComponents.Get(cellEntity).Tripos;
                map.Add(tripos, occupiedEntity);
            }

            return map;
        }

    }
}
