using Scellecs.Morpeh;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public interface IMovementCellsMap
    {
        bool TryGetValue(IntTriangularPos tripos, out CellMovementData cellData);
        void Clear();
        bool TryWriteCell(IntTriangularPos tripos, CellMovementData cellData);
        bool TryWriteCell(IntTriangularPos tripos, Entity entity, float2 moveDir, int projectionIndex);
    }
}
