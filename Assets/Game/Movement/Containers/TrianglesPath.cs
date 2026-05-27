using ZE.MechBattle.Navigation;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class TrianglesPath : PathData<IntTriangularPos, IntTriangularPos>, ICalculationSystemPath
    {
        public TrianglesPath(int id, (IntTriangularPos, IntTriangularPos) destinationKey) : base(id, destinationKey)
        {
        }

        int ICalculationSystemPath.Id => Id;
    }
}
