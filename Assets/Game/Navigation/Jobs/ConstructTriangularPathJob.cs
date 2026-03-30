using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace ZE.MechBattle.Navigation
{
    [BurstCompile]
    public struct ConstructTriangularPathJob : IJob
    {
        [ReadOnly] public SquaredHexTrianglesList<TriangleNavData> SetupData;

        public IntTriangularPos Start;
        public IntTriangularPos End;

        [NoAlias] public NativeArray<AstarPathNodeData<IntTriangularPos>> CalculationData;
        [NoAlias] public NativeList<IntTriangularPos> ResultList;
        [NoAlias] public NativeHashSet<int> OpenedList;
        private const int EDGE_PASS_COST = 1;
        private const int VERTEX_PASS_COST = 2;

        public void Execute()
        {
            OpenedList.Clear();
            ResultList.Clear();
            for (var i = 0; i < CalculationData.Length; i++)
            {
                CalculationData[i] = CalculationData[i].Reset();
            }


            if (!SetupData.TryGetIndex(Start, out var startTriangleIndex))
            {
                #if UNITY_EDITOR
                Debug.Log("start pos not valid");
                #endif
                return;
            }    

            var closestDistance = TriangularMath.CalculateDistance(Start, End);
            var closestNode = Start;

            for (var i = 0; i < CalculationData.Length; i++)
            {
                var data = CalculationData[i];
                data.HeuristicCost = TriangularMath.CalculateDistance(data.NodeKey, Start);
                CalculationData[i] = data;
            }

            // start
            AstarLogic.SetupStartCell(startTriangleIndex, CalculationData);
            HandleNeighbours(Start, startTriangleIndex);

            do
            {
                var nextNode = AstarLogic.FindNextNode(OpenedList, CalculationData);
                if (nextNode.value == End)
                {
                    closestNode = End;
                    break;
                }
                else
                {
                    var dist = TriangularMath.CalculateDistance(nextNode.value, End);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closestNode = nextNode.value;
                    }
                }

                HandleNeighbours(nextNode.value, nextNode.index);
            }
            while (OpenedList.Count != 0);

            BuildPath(closestNode);
        }

        private void HandleNeighbours(IntTriangularPos pos, int index)
        {
            var activeNodeData = CalculationData[index];
            var coordsConverter = SetupData.CoordsConverter;
            for (var i = 0; i < 12; i++)
            {
                var neighbourPos = TriangularMath.GetNeighbourByDirection(pos, i);
                var data = SetupData.GetValidOrDefault(neighbourPos);
                if (!data.IsValid | !data.IsPassable)
                    continue;

                var edgesMask = math.select(VERTEX_PASS_COST, EDGE_PASS_COST, neighbourPos.IsPeak);
                var cost = math.select(VERTEX_PASS_COST, EDGE_PASS_COST, ((1 << i) & edgesMask) != 0);
                AstarLogic.HandleNeighbour(activeNodeData, coordsConverter.TriangularToIndex(neighbourPos), OpenedList, CalculationData, cost);
            }
        }

        private void BuildPath(IntTriangularPos finalPos)
        {
            var coordsConverter = SetupData.CoordsConverter;

            var index = coordsConverter.TriangularToIndex(finalPos);
            var finalNodeData = CalculationData[index];
            var stepsCount = finalNodeData.StepsCount;
            //PathCost.Value = finalNodeData.PathCost;
            ResultList.Resize(stepsCount + 1, NativeArrayOptions.UninitializedMemory);

            var currentPos = finalPos;
            var i = stepsCount;
            while (i >= 0)
            {
                ResultList[i--] = currentPos;
                var data = CalculationData[coordsConverter.TriangularToIndex(currentPos)];
                currentPos = data.ParentNodeKey;
            }

            for (var j = 0; j < CalculationData.Length; j++)
            {
                var setupData = SetupData[j];
                if (setupData.IsValid)
                    continue;

                //Debug.Log($"{coordsConverter.IndexToTriangular(j)}: {CalculationData[j].PathCost}");
            }
        }
    }
}
