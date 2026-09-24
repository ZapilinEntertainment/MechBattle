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
            public int Direction;
            public int OriginalDirectionDelta;

            public NextTriangleOption(float dotValue, float densityValue, IntTriangularPos tripos, int direction, int originalDirection)
            {
                DotValue = dotValue;
                DensityValue = densityValue;
                Tripos = tripos;
                Direction = direction;
                OriginalDirectionDelta = TriangularMath.GetDirectionsDelta(originalDirection, direction);
            }
        }

        private struct DensityMapChecker<T> where T : ITriangleNeighbourOffsets
        {
            private readonly T _offsets;
            private readonly IMovementDensityMap _densityMap;
            private readonly INavigationMap _navigationMap;

            public DensityMapChecker(T offsets, IMovementDensityMap densityMap, INavigationMap navigationMap)
            {
                _offsets = offsets;
                _densityMap = densityMap;
                _navigationMap = navigationMap;
            }

            public NextTriangleOption SearchForBestOption(int originalDirection, IntTriangularPos tripos)
            {
                var bestOption = new NextTriangleOption() { 
                    DensityValue = float.MaxValue, 
                    DotValue = float.MinValue, 
                    Tripos = tripos,
                    Direction = originalDirection,
                    OriginalDirectionDelta = int.MaxValue};

                var originalDirectionVector = math.normalize(_offsets[originalDirection]);
                var direction = -1;

                foreach (var neighbourPos in new TriangleNeighboursEnumerator<T>(tripos, _offsets))
                {
                    direction++;

                    if (!_navigationMap.IsCellPassable(neighbourPos))
                         continue;

                    if (!_densityMap.TryGetDensity(neighbourPos, out var density))
                        density = 0f;

                    var currentDirVector = math.normalize(_offsets[direction]);
                    var dot = math.dot(originalDirectionVector, currentDirVector);
                    var option = new NextTriangleOption(dot, density, neighbourPos, direction, originalDirection);
                    //UnityEngine.Debug.Log($"{direction} : score {option.ScorePoints} : dot {option.DotValue} : density {option.DensityValue}");

                    var comparationResult = option.DensityValue.CompareTo(bestOption.DensityValue);
                    if (comparationResult == 1)
                        continue;
                    if (comparationResult == -1)
                    {
                        bestOption = option;
                    }
                    else
                    {
                        if (option.OriginalDirectionDelta < bestOption.OriginalDirectionDelta)
                            bestOption = option;
                    }
                }

                return bestOption;
            }
        }

        public static NextTriangleOption Execute(IntTriangularPos tripos, IMovementDensityMap densityMap, INavigationMap navigationMap, int originalDirection)
        {
            NextTriangleOption bestOption;

            if (tripos.IsPeak)
            {
                var checker = new DensityMapChecker<PeakNeighbourOffsets>(new PeakNeighbourOffsets(), densityMap, navigationMap);
                bestOption = checker.SearchForBestOption(originalDirection, tripos);
            }
            else
            {
                var checker = new DensityMapChecker<ValleyNeighbourOffsets>(new ValleyNeighbourOffsets(), densityMap, navigationMap);
                bestOption = checker.SearchForBestOption(originalDirection, tripos);
            }

            return bestOption;
        }

    }
}
