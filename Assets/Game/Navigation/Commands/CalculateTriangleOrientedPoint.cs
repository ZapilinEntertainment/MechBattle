using Unity.Mathematics;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    // returns triangle plane for height ransac operations
    public static class CalculateTriangleOrientedPoint
    {
        [BurstCompile]
        public static OrientedPoint Execute(IntTriangularPos tripos, float triangleHeight, CellHeightData heightData)
        {
            var vertices = GetTriangleVerticesCommand.Execute(tripos, triangleHeight, heightData);
            var dirAB = math.normalize(vertices.LeftBasisPos - vertices.PinnaclePos);
            var dirAC = math.normalize(vertices.RightBasisPos - vertices.PinnaclePos);
            var normal = math.normalize( tripos.IsPeak ? math.cross(dirAC, dirAB) : math.cross(dirAB, dirAC));
            var pos = TriangularMath.TriangularToWorld(tripos, triangleHeight);
            return new(pos, normal);
        }
    
    }
}
