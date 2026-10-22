using Unity.Burst;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class AffectedTrianglesCalculator
    {
        public interface IAffectedCellsOperator
        {
            void OnCellAffected(IntTriangularPos tripos);
        }

        [BurstCompile]
        public static int CalculateRectOutlinedRadius(float2 rectSize, float triangleHeight)
        {
            var rectOutlinedRadius = math.sqrt(rectSize.x * rectSize.x + rectSize.y + rectSize.y) * 0.5f;
            return MathExtensions.UnitsRadiusToTriangular(rectOutlinedRadius, triangleHeight);
        }

        [BurstCompile]
        public static void CalculateRectAffectedTriangles<T>(
            T cellOperator,
            RigidTransform rectPoint, 
            float2 rectSize, 
            float triangleHeight, 
            int checkHexRadius)

            where T : IAffectedCellsOperator
        {
            var stepTripos = TriangularMath.WorldToTrianglePos(rectPoint.pos, triangleHeight);
            var virtualHexCenter = GetClosestVertexTriposCommand.Execute(rectPoint.pos, triangleHeight, stepTripos);

            var invertedRotation = math.inverse(rectPoint.rot);

            foreach (var tripos in new HexTrianglesEnumerator(virtualHexCenter, checkHexRadius))
            {
                var worldPos = TriangularMath.TriangularToWorld(tripos, triangleHeight);
                var inversedTranslation = worldPos - rectPoint.pos;
                var localPos = math.mul(invertedRotation, inversedTranslation);

                if (math.abs(localPos.x) < rectSize.x && math.abs(localPos.z) < rectSize.y)
                {
                    cellOperator.OnCellAffected(tripos);
                }
            }
        }
    
    }
}
