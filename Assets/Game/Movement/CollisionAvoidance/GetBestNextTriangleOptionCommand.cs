using System;
using Unity.Burst;
using Unity.Collections;
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

        [BurstDiscard]
        public static NextTriangleOption Execute(
            IntTriangularPos tripos, 
            IMovementDensityMap densityMap, 
            INavigationMap navigationMap, 
            int originalDirection)
        {
            NextTriangleOption bestOption;

            if (tripos.IsPeak)
            {
                bestOption = SearchForBestOption(originalDirection, tripos, new PeakNeighbourOffsets(), navigationMap, densityMap);
            }
            else
            {
                bestOption = SearchForBestOption(originalDirection, tripos, new ValleyNeighbourOffsets(), navigationMap, densityMap);
            }

            return bestOption;
        }

        [BurstCompile]
        public static NextTriangleOption Execute(
            IntTriangularPos tripos, 
            in NativeParallelHashMap<IntTriangularPos, AvoidanceMapData>.ReadOnly avoidanceData, 
            int originalDirection)
        {
            NextTriangleOption bestOption;

            if (tripos.IsPeak)
            {
                bestOption = SearchForBestOption(originalDirection, tripos, new PeakNeighbourOffsets(), avoidanceData);
            }
            else
            {
                bestOption = SearchForBestOption(originalDirection, tripos, new ValleyNeighbourOffsets(), avoidanceData);
            }

            return bestOption;
        }

        // there is better be a wrapper to unify functional, however it is no possible to mix managed and unmanaged versions
       
        [BurstCompile]
        private static NextTriangleOption SearchForBestOption<T>(
            int originalDirection, 
            IntTriangularPos tripos, 
            T offsets,
             in NativeParallelHashMap<IntTriangularPos, AvoidanceMapData>.ReadOnly avoidanceData
            ) where T : unmanaged, ITriangleNeighbourOffsets
        {
            Span<(IntTriangularPos, AvoidanceMapData)> neighboursData = stackalloc (IntTriangularPos, AvoidanceMapData)[12];
            var i = 0;
            foreach (var neighbourPos in new TriangleNeighboursEnumerator<T>(tripos, offsets))
            {
                avoidanceData.TryGetValue(neighbourPos, out var neighbourAvoidanceData);
                // default value is ok (passable = false)
                neighboursData[i++] = (neighbourPos, neighbourAvoidanceData);
            }

            return SelectBestTriangleOption(tripos, originalDirection, offsets, neighboursData);
        }

        [BurstDiscard]
        private static NextTriangleOption SearchForBestOption<T>(
           int originalDirection,
           IntTriangularPos tripos,
           T offsets,
           INavigationMap navigationMap,
           IMovementDensityMap densityMap
           ) where T : unmanaged, ITriangleNeighbourOffsets
        {
            Span<(IntTriangularPos, AvoidanceMapData)> neighboursData = stackalloc (IntTriangularPos, AvoidanceMapData)[12];
            var i = 0;
            foreach (var neighbourPos in new TriangleNeighboursEnumerator<T>(tripos, offsets))
            {
                var passabilityData = navigationMap.GetPassabilityData(neighbourPos);
                neighboursData[i++] = (neighbourPos, new(passabilityData, densityMap.GetMovementDensity(neighbourPos)));
            }

            return SelectBestTriangleOption(tripos, originalDirection, offsets, neighboursData);
        }

        [BurstCompile]
        private static NextTriangleOption SelectBestTriangleOption<T>(
            IntTriangularPos startPos,
            int originalDirection,
            T offsets,
            Span<(IntTriangularPos, AvoidanceMapData)> list)
             where T : unmanaged, ITriangleNeighbourOffsets
        {
            var originalDirectionVector = math.normalize(offsets[originalDirection]);
            var bestOption = new NextTriangleOption()
            {
                DensityValue = float.MaxValue,
                DotValue = float.MinValue,
                Tripos = startPos,
                Direction = originalDirection,
                OriginalDirectionDelta = int.MaxValue
            };

            for (var i = 0; i < list.Length; i++)
            {
                var (neighbourPos, avoidanceData) = list[i];
                if (!avoidanceData.IsPassable || ((avoidanceData.NeighboursAccessMask & (1 << i)) == 0))
                    continue;

                var currentDirVector = math.normalize(offsets[i]);
                var dot = math.dot(originalDirectionVector, currentDirVector);
                var option = new NextTriangleOption(dot, avoidanceData.MovementDensity, neighbourPos, i, originalDirection);
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
}
