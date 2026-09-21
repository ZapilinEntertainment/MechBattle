using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public static class GetBestNextTriangleOptionCommand
    {
        public struct NextTriangleOption
        {
            public float DotValue;
            public float DensityValue;
            public IntTriangularPos Tripos;
            public float ScorePoints;
            public int Direction;

            public NextTriangleOption(float dotValue, float densityValue, IntTriangularPos tripos, int direction)
            {
                DotValue = dotValue;
                DensityValue = densityValue;
                Tripos = tripos;
                Direction = direction;
                ScorePoints = CalculateScorePoints(dotValue, densityValue);
            }

            private static float CalculateScorePoints(float dotValue, float densityValue) => dotValue * 100f / (densityValue * densityValue);
        }

        private struct DensityMapChecker<T> where T : ITriangleNeighbourOffsets
        {
            private readonly T _offsets;
            private readonly MovementDensityMap _densityMap;

            public DensityMapChecker(T offsets, MovementDensityMap densityMap)
            {
                _offsets = offsets;
                _densityMap = densityMap;
            }

            public NextTriangleOption SearchForBestOption(int originalDirection, IntTriangularPos tripos)
            {
                var bestOption = new NextTriangleOption() { 
                    DensityValue = float.MaxValue, 
                    DotValue = float.MinValue, 
                    ScorePoints = float.MinValue,
                    Tripos = tripos,
                    Direction = originalDirection};

                var originalDirectionVector = _offsets[originalDirection];
                var direction = -1;

                foreach (var neighbourPos in new TriangleNeighboursEnumerator<T>(tripos, _offsets))
                {
                    direction++;
                    if (!_densityMap.TryGetDensity(neighbourPos, out var density))
                        continue;

                    var currentDirVector = _offsets[direction];
                    var dot = math.dot(originalDirection, currentDirVector);
                    var option = new NextTriangleOption(dot, density, neighbourPos, direction);

                    if (option.ScorePoints > bestOption.ScorePoints)
                        bestOption = option;
                }

                return bestOption;
            }
        }

        public static NextTriangleOption Execute(IntTriangularPos tripos, FlowMap flowMap, MovementDensityMap densityMap, int originalDirection)
        {
            NextTriangleOption bestOption;

            if (tripos.IsPeak)
            {
                var checker = new DensityMapChecker<PeakNeighbourOffsets>(new PeakNeighbourOffsets(), densityMap);
                bestOption = checker.SearchForBestOption(originalDirection, tripos);
            }
            else
            {
                var checker = new DensityMapChecker<ValleyNeighbourOffsets>(new ValleyNeighbourOffsets(), densityMap);
                bestOption = checker.SearchForBestOption(originalDirection, tripos);
            }

            return bestOption;
        }

    }
}
