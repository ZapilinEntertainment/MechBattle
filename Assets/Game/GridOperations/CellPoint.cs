using ZE.MechBattle.Navigation;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public struct CellPoint
    {
        public IntTriangularPos Tripos;
        public int Direction;
    

        public RigidTransform ToRigidTransform(float triangleHeight) =>
            new(translation: TriangularMath.TriangularToWorld(Tripos, triangleHeight),
                rotation: Tripos.IsPeak ? ((PeakNeighbour)Direction).ToRotation() : ((ValleyNeighbour)Direction).ToRotation()
                );
    }
}
