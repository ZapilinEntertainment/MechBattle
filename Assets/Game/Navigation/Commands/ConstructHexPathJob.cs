using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle.Navigation
{

    [BurstCompile]
    public struct ConstructHexPathJob : IJob
    {
        [ReadOnly] public NativeHashMap<int2, NavigationNodeData> InitialData;
        [WriteOnly] public NativeList<int2> ResultingData;

        public int2 StartPos;
        public int2 TargetPos;
        public NativeHashMap<int2, CalculatedNavigationData> CalculatedData;
        public NativeHashSet<int2> OpenedHexes;
        public NativeHashSet<int2> ClosedHexes;
        private const int DEFAULT_STEP_COST = 1;

        public void Execute()
        {
            OpenedHexes.Clear();
            ClosedHexes.Clear();
            CalculatedData.Clear();
            ResultingData.Clear();

            OpenedHexes.Add(StartPos);
            CalculatedData[StartPos] = new()
            {
                Cost = InitialData[StartPos].HeuristicCost,
                Parent = int2.zero,
                StepsCount = 0
            };

            var currentHexPos = StartPos;
            var prevHexPos = currentHexPos;

            while (OpenedHexes.Count != 0)
            {
                var minDist = int.MaxValue;

                foreach (var hexPos in OpenedHexes)
                {
                    var fsum = CalculatedData[hexPos].Cost + InitialData[hexPos].HeuristicCost;
                    if (fsum < minDist)
                    {
                        minDist = fsum;
                        currentHexPos = hexPos;
                    }
                }
                //Debug.Log($"goto {currentHexPos}");

                // check if completed

                if (math.all(currentHexPos == TargetPos))
                {
                    CalculatedData[currentHexPos] = new()
                    {
                        Cost = minDist,
                        Parent = prevHexPos,
                        StepsCount = CalculatedData[prevHexPos].StepsCount + 1
                    };
                    break;
                }

                OpenedHexes.Remove(currentHexPos);
                ClosedHexes.Add(currentHexPos);
                prevHexPos = currentHexPos;

                //checking neighbours:
                var currentHexNode = InitialData[currentHexPos];
                for (var i = 0; i < 6; i++)
                {
                    var edge = (HexEdge)i;
                    if (!currentHexNode.IsEdgePassable(edge))
                        continue;
                    var neighbourPos = currentHexPos + edge.ToOffsetVector();
                    //Debug.Log($"{neighbourPos} : {edge} : {InitialData.TryGetValue(neighbourPos, out var testNode)} : {!ClosedHexes.Contains(neighbourPos)} : {testNode.IsEdgePassable(edge.ToOpposite())}");

                    if (!InitialData.TryGetValue(neighbourPos, out var neighbourNode)
                        || ClosedHexes.Contains(neighbourPos)
                        || !neighbourNode.IsEdgePassable(edge.ToOpposite()))
                        continue;

                    var newNeighbourCost = minDist + DEFAULT_STEP_COST;
                    OpenedHexes.Add(neighbourPos);

                    var stepsCount = CalculatedData[currentHexPos].StepsCount + 1;
                    if (!CalculatedData.TryGetValue(neighbourPos, out var neighbourData)
                        || neighbourData.Cost > newNeighbourCost)
                    {
                        CalculatedData[neighbourPos] = new()
                        {
                            Cost = newNeighbourCost,
                            Parent = currentHexPos,
                            StepsCount = stepsCount
                        };
                    }
                }
            }

            BuildPath(currentHexPos);
        }

        void BuildPath(int2 finalPos)
        {
            var stepsCount = CalculatedData[finalPos].StepsCount;
            ResultingData.Resize(stepsCount+1, NativeArrayOptions.UninitializedMemory);

            var currentPos = finalPos;
            var i = stepsCount;
            while (i != 0)
            {
                ResultingData[i--] = currentPos;

                var data = CalculatedData[currentPos];
                currentPos = data.Parent;
            }
            ResultingData[0] = StartPos;
        }
    }
}
