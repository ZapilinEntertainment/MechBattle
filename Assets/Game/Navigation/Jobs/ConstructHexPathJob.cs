using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle.Navigation
{
    // NOTE:
    // this a-star algorithm build path based on hex edge centers
    // every hex have 6 edge points, however each one is its neighbour counter-edge
    // so HexData contains exact hex edge access masks and passabilities,
    // when its edges are set as indices in NavigationData - so edge points will not be doubled


    [BurstCompile]
    public struct ConstructHexPathJob : IJob
    {
        [WriteOnly] public NativeList<HexPathNodeKey> ResultingData;

        public HexPathNodeKey Start;
        public HexPathNodeKey End;
        public NativeReference<float> PathCost;
       
        [ReadOnly] public NativeHashMap<int2, HexEdgeNodesData> HexData;
        [NoAlias] public NativeHashSet<int> OpenedList;
        [NoAlias] public NativeArray<AstarPathNodeData<HexPathNodeKey>> NavigationData;

        private const int DEFAULT_PATH_COST = 1;

        public void Execute()
        {
            OpenedList.Clear();
            ResultingData.Clear();
            for (var i = 0; i < NavigationData.Length; i++)
            {
                NavigationData[i] = NavigationData[i].Reset();
            }
            PathCost.Value = 0f;


            //Debug.Log($"navigation data length: {NavigationData.Length}");
            var startData = HexData[Start.HexCoord];
            var closestDistance = HexMath.CalculateDistance(Start, End);
            var closestNode = Start;           

            for (var i = 0; i < NavigationData.Length; i++)
            {
                var data = NavigationData[i];
                data.HeuristicCost = HexMath.CalculateDistance(data.NodeKey.HexCoord,End.HexCoord);
                NavigationData[i] = data;
            }

            // start
            var startDataIndex = startData.GetNodeIndex(Start.EdgeIndex);
            AstarLogic.SetupStartCell(startDataIndex, NavigationData);
            HandleNeighbours(Start);

            //UnityEngine.Debug.Log($"search for {Start} -> {End}");
            do
            {
                var nextNode = AstarLogic.FindNextNode(OpenedList, NavigationData);
                //UnityEngine.Debug.Log($"next {nextNode}");
                if (nextNode.value == End)
                {
                    //UnityEngine.Debug.Log($"exit found: {nextNode}");
                    closestNode = End;
                    break;          
                }
                else
                {
                    var dist = HexMath.CalculateDistance(nextNode.value, End);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closestNode = nextNode.value;
                    }
                }

                HandleNeighbours(nextNode.value);
            }
            while (OpenedList.Count != 0);

            BuildPath(closestNode);
        }

        private void BuildPath(HexPathNodeKey finalPos)
        {
            var index = GetNodeIndex(finalPos);
            var finalNodeData = NavigationData[index];
            var stepsCount = finalNodeData.StepsCount;
            PathCost.Value = finalNodeData.CostFromStart;
            ResultingData.Resize(stepsCount+1, NativeArrayOptions.UninitializedMemory);

            var currentPos = finalPos;
            var i = stepsCount;
            while (i >= 0)
            {
                ResultingData[i--] = currentPos;

                var data = GetNavData(currentPos);
                currentPos = data.ParentNodeKey;
            }
        }

        private void HandleNeighbours(HexPathNodeKey activeNodePos)
        {
            //own hex nodes:
            var hexData = HexData[activeNodePos.HexCoord];
            var activeNodeData = NavigationData[hexData.GetNodeIndex(activeNodePos.EdgeIndex)];

            //Debug.Log($"updating {activeNodePos} neighbours:");

            for (var i = 0; i < 6; i++)
            {
                if (!hexData.TryGetNodeIndex(i, out var neighbourIndex)
                    || !hexData.AccessMap.AreEdgesConnected(activeNodePos.Edge, (HexEdge)(i)))
                    continue;

                AstarLogic.HandleNeighbour(activeNodeData,
                    neighbourIndex,
                    OpenedList,
                    NavigationData,
                    DEFAULT_PATH_COST);
            }

            // neighboured hex:
            var neighbouredHexPos = activeNodePos.ToOppositeHexCoord();
            if (!hexData.IsEdgePassable(activeNodePos.Edge) 
                || !HexData.TryGetValue(neighbouredHexPos, out var neighbouredHexData)
                || !neighbouredHexData.IsEdgePassable(activeNodePos.Edge.ToOpposite()))
                return;

            var edgeInNeighbouredHex = activeNodePos.Edge.ToOpposite();
            for (var i = 0; i < 6; i++)
            {
                if (!neighbouredHexData.TryGetNodeIndex(i, out var neighbourIndex)
                    ||!neighbouredHexData.AccessMap.AreEdgesConnected(edgeInNeighbouredHex, (HexEdge)i))
                    continue;

                AstarLogic.HandleNeighbour(
                    activeNodeData,
                    neighbourIndex,
                    OpenedList,
                    NavigationData,
                    DEFAULT_PATH_COST);
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AstarPathNodeData<HexPathNodeKey> GetNavData(HexPathNodeKey key) => NavigationData[GetNodeIndex(key)];


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNodeIndex(HexPathNodeKey key) => HexData[key.HexCoord].GetNodeIndex(key.EdgeIndex);
    }
}
