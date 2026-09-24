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

            private static float CalculateScorePoints(float dotValue, float densityValue) => dotValue * 100f / ((1f+densityValue) * (1f+densityValue));
        }

        private struct DensityMapChecker<T> where T : ITriangleNeighbourOffsets
        {
            private readonly T _offsets;
            private readonly IMovementDensityMap _densityMap;

            public DensityMapChecker(T offsets, IMovementDensityMap densityMap)
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

                var originalDirectionVector = math.normalize(_offsets[originalDirection]);
                var direction = -1;

                foreach (var neighbourPos in new TriangleNeighboursEnumerator<T>(tripos, _offsets))
                {
                    direction++;
                    if (!_densityMap.TryGetDensity(neighbourPos, out var density))
                        density = 0f;

                    var currentDirVector = math.normalize(_offsets[direction]);
                    var dot = math.dot(originalDirectionVector, currentDirVector);
                    var option = new NextTriangleOption(dot, density, neighbourPos, direction);
                    //UnityEngine.Debug.Log($"{direction} : score {option.ScorePoints} : dot {option.DotValue} : density {option.DensityValue}");

                    // yes, there is score logics, hovewer simple density check works better
                    // todo: investigate
                    if (option.DensityValue < bestOption.DensityValue)
                    {
                        bestOption = option;
                    }
                }

                return bestOption;
            }
        }

        public static NextTriangleOption Execute(IntTriangularPos tripos, IMovementDensityMap densityMap, int originalDirection)
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
