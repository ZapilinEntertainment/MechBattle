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
                data.HeuristicCost = HexMath.CalculateDistance(data.NodeKey, End);
                NavigationData[i] = data;
                //UnityEngine.Debug.Log($"[{i}]: {data.HeuristicCost}");
            }

            var endIndex = HexData[End.HexCoord].GetNodeIndex(End.EdgeIndex);
            //UnityEngine.Debug.Log($"target node [{endIndex}] cost: {NavigationData[ endIndex].TotalPathCost}");

            // start
            var startDataIndex = startData.GetNodeIndex(Start.EdgeIndex);
#if UNITY_EDITOR
            if (startDataIndex == -1)
                throw new System.Exception($"{Start}->{End} hex path search job: invalid start index");
#endif
            AstarLogic.SetupStartCell(startDataIndex, NavigationData);
            HandleNeighbours(Start);

            //UnityEngine.Debug.Log($"search for {Start} -> {End}");


            var endOpposite = End.ToOpposite();
            do
            {
                var nextNode = AstarLogic.FindNextNode(OpenedList, NavigationData);
                if (nextNode.value == End | nextNode.value == endOpposite)
                {
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
            var finalNodeIndex = GetNodeIndex(finalPos);
            var finalNodeData = NavigationData[finalNodeIndex];

            var stepsCount = finalNodeData.StepsCount;
            PathCost.Value = finalNodeData.CostFromStart;
            ResultingData.Resize(stepsCount + 1, NativeArrayOptions.UninitializedMemory);

            var currentPos = finalPos;
            var i = stepsCount;

            if ((Start.Edge == HexEdge.BottomRight || Start.Edge == HexEdge.Top || Start.Edge == HexEdge.TopRight) && End.Edge == HexEdge.TopLeft) 
            {
                UnityEngine.Debug.Log($"{Start.Edge} -> {End.Edge}");
            }

            while (i >= 0)
            {
                var prevpos = currentPos;
                ResultingData[i--] = currentPos;

                var data = GetNavData(currentPos);
                currentPos = data.ParentNodeKey;

                if ((Start.Edge == HexEdge.BottomRight || Start.Edge == HexEdge.Top || Start.Edge == HexEdge.TopRight) && End.Edge == HexEdge.TopLeft)
                {
                    UnityEngine.Debug.Log($"{i + 1}: {prevpos} :{data.CostFromStart}");
                }
                    
            }
        }


        private void HandleNeighbours(HexPathNodeKey activeNode)
        {
            /*
             *  Example: active node is (a,b) BottomRight / (a+1, b-1) Top
             *  so we check firstly own hex nodes (O-marked)
             *  and then neighbour hex nodes (N-marked)
             * 
                           TopO
              TopLeftO           TopRightO                             
              BottomLeftO       -> ACTIVE NODE<-         
                    BottomO(TopLeftN)            TopRightN
                       BottomLeftN                  BottomRightN
                                     BottomN
            */
            //own hex nodes:
            var hexData = HexData[activeNode.HexCoord];
            var activeNodeData = NavigationData[hexData.GetNodeIndex(activeNode.EdgeIndex)];

            //UnityEngine.Debug.Log($"active node: {activeNode}, cost: {activeNodeData.TotalPathCost} ");

            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                if (!hexData.TryGetNodeIndex(i, out var neighbourIndex)
                    || !hexData.AccessMap.AreEdgesConnected(activeNode.Edge, edge))
                    continue;

                AstarLogic.HandleNeighbour(activeNodeData,
                    neighbourIndex,
                    OpenedList,
                    NavigationData,
                    activeNode.Edge.GetInHexTransitionCost(edge));


                //UnityEngine.Debug.Log($"{activeNode} ->  {new HexPathNodeKey(activeNode.HexCoord, i)} ( [{neighbourIndex}] cost: {NavigationData[neighbourIndex].CostFromStart} / {NavigationData[neighbourIndex].TotalPathCost})");
            }

            // neighboured hex nodes:
            var neighbourHexNode = activeNode.ToOpposite();
            if (!hexData.IsEdgePassable(activeNode.Edge)
                || !HexData.TryGetValue(neighbourHexNode.HexCoord, out var neighbouredHexData)
                || !neighbouredHexData.IsEdgePassable(neighbourHexNode.Edge))
                return;

            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;

                //UnityEngine.Debug.Log($"{neighbourHexNode} -> {edge} = {neighbouredHexData.TryGetNodeIndex(i, out var ni)} {ni} = connected: {neighbouredHexData.AccessMap.AreEdgesConnected(neighbourHexNode.Edge, edge)}");

                if (!neighbouredHexData.TryGetNodeIndex(i, out var neighbourIndex)
                    || !neighbouredHexData.AccessMap.AreEdgesConnected(neighbourHexNode.Edge, edge))
                    continue;

                AstarLogic.HandleNeighbour(
                    activeNodeData,
                    neighbourIndex,
                    OpenedList,
                    NavigationData,
                    neighbourHexNode.Edge.GetInHexTransitionCost(edge));

                //UnityEngine.Debug.Log($"{activeNode} ->  {new HexPathNodeKey(neighbourHexNode.HexCoord, i)}  ( [{neighbourIndex}] cost:{NavigationData[neighbourIndex].CostFromStart} / {NavigationData[neighbourIndex].TotalPathCost})");
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AstarPathNodeData<HexPathNodeKey> GetNavData(HexPathNodeKey key) => NavigationData[GetNodeIndex(key)];


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNodeIndex(HexPathNodeKey key) => HexData[key.HexCoord].GetNodeIndex(key.EdgeIndex);
    }
}