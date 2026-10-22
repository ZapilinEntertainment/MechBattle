using Unity.Burst;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class AffectedTrianglesCalculator
    {
        public interface IAffectedCellsOperator
        {
            void AddCell(IntTriangularPos tripos);
        }


        [BurstCompile]
        public static int CalculateRectOutlinedRadius(float2 rectSize, float triangleHeight)
        {
            var rectOutlinedRadius = math.sqrt(rectSize.x * rectSize.x + rectSize.y + rectSize.y) * 0.5f;
            return MathExtensions.UnitsRadiusToTriangular(rectOutlinedRadius, triangleHeight);
        }

        [BurstDiscard]
        public static void CalculateRectAffectedTrianglesManaged<T>(
            T cellOperator,
            RigidTransform rectPoint, 
            float2 rectSize, 
            float triangleHeight, 
            int checkHexRadius)
            where T : IAffectedCellsOperator
        {
            #region CalculateRectAffectedTriangles - source

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
                    cellOperator.AddCell(tripos);
                }
            }
            #endregion
        }

        [BurstCompile]
        public static void CalculateRectAffectedTrianglesUnmanaged<T>(
          T cellOperator,
          RigidTransform rectPoint,
          float2 rectSize,
          float triangleHeight,
          int checkHexRadius)
      where T : unmanaged, IAffectedCellsOperator
        {
            #region CalculateRectAffectedTriangles - copy

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
                    cellOperator.AddCell(tripos);
                }
            }
            #endregion
        }

        [BurstCompile]
        public static void CalculateRoundAffectedTrianglesUnmanaged<T>(T cellOperator, float3 worldPos, float radiusInUnits, float triangleHeight)
            where T : unmanaged, IAffectedCellsOperator
        {
            #region CalculateRoundAffectedTriangles - source
            var tripos = TriangularMath.WorldToTrianglePos(worldPos, triangleHeight);
            var virtualHexCenter = GetClosestVertexTriposCommand.Execute(worldPos, triangleHeight, tripos);
            var radius = MathExtensions.UnitsRadiusToTriangular(radiusInUnits, triangleHeight);

            var unitsRadiusSq = radiusInUnits * radiusInUnits;
            var count = 0;
            foreach (var pos in new HexTrianglesEnumerator(virtualHexCenter, radius))
            {
                var trianglePos = TriangularMath.TriangularToWorld(pos, triangleHeight);
                if (math.distancesq(trianglePos, worldPos) < unitsRadiusSq)
                {
                    count++;
                    cellOperator.AddCell(pos);
                }                    
            }

            if (count == 0)
                cellOperator.AddCell(tripos);
            #endregion
        }


        [BurstDiscard]
        public static void CalculateRoundAffectedTrianglesManaged<T>(T cellOperator, float3 worldPos, float radiusInUnits, float triangleHeight)
            where T : IAffectedCellsOperator
        {
            #region CalculateRoundAffectedTriangles - copy
            var tripos = TriangularMath.WorldToTrianglePos(worldPos, triangleHeight);
            var virtualHexCenter = GetClosestVertexTriposCommand.Execute(worldPos, triangleHeight, tripos);
            var radius = MathExtensions.UnitsRadiusToTriangular(radiusInUnits, triangleHeight);

            var unitsRadiusSq = radiusInUnits * radiusInUnits;
            var count = 0;
            foreach (var pos in new HexTrianglesEnumerator(virtualHexCenter, radius))
            {
                var trianglePos = TriangularMath.TriangularToWorld(pos, triangleHeight);
                if (math.distancesq(trianglePos, worldPos) < unitsRadiusSq)
                {
                    count++;
                    cellOperator.AddCell(pos);
                }
            }

            if (count == 0)
                cellOperator.AddCell(tripos);
            #endregion
        }
    }
}
