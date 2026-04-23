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
        [ReadOnly] public FlattenedHexList<CellPassabilityData> PassabilityData;

        public IntTriangularPos Start;
        public IntTriangularPos End;

        [NoAlias] public NativeArray<AstarPathNodeData<IntTriangularPos>> CalculationData;
        [NoAlias] public NativeList<IntTriangularPos> ResultList;
        [NoAlias] public NativeHashSet<int> OpenedList;

        public void Execute()
        {
            OpenedList.Clear();
            ResultList.Clear();
            for (var i = 0; i < CalculationData.Length; i++)
            {
                CalculationData[i] = CalculationData[i].Reset();
            }


            if (!PassabilityData.TryGetIndex(Start, out var startTriangleIndex))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("start pos not valid");
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
            var isPeak = pos.IsPeak;

            for (var neighbourDirection = 0; neighbourDirection < 12; neighbourDirection++)
            {
                if (!PassabilityData[index].IsNeighbourAccessible(neighbourDirection))
                    continue;

                var neighbourPos = TriangularMath.GetNeighbourByDirection(pos, neighbourDirection);
                var isIndexValid = PassabilityData.TryGetValue(neighbourPos, out var neighbourPassabilityData, out var neighbourIndex);
                
                // not passable by default
                if (!isIndexValid | !neighbourPassabilityData.IsPassable)
                    continue;

                var cost = TriangularMath.GetTransitionCost(neighbourDirection, isPeak);
                AstarLogic.HandleNeighbour(activeNodeData, neighbourIndex, OpenedList, CalculationData, cost);
            }
        }

        private void BuildPath(IntTriangularPos finalPos)
        {
            var index = PassabilityData.TriangularToIndex(finalPos);
            var finalNodeData = CalculationData[index];
            var stepsCount = finalNodeData.StepsCount;
            //PathCost.Value = finalNodeData.PathCost;
            ResultList.Resize(stepsCount + 1, NativeArrayOptions.UninitializedMemory);

            var currentPos = finalPos;
            var i = stepsCount;
            while (i >= 0)
            {
                ResultList[i--] = currentPos;
                var data = CalculationData[PassabilityData.TriangularToIndex(currentPos)];
                currentPos = data.ParentNodeKey;
                //UnityEngine.Debug.Log(data.PathCost);
            }
        }
    }
}
