using System;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class HexPathJobCollections : IDisposable
    {
        public NativeHashMap<int2, HexEdgeNodesData> HexData;
        public NativeHashSet<int> OpenedList;
        public NativeArray<NavigationHexNodeData> NavigationData;
        public NativeList<HexPathNodeKey> ResultingData;
        public NativeReference<float> PathCost;

        public HexPathJobCollections(Allocator allocator, int hexesCount)
        {
            var pointsCount = hexesCount * 6;
            HexData = new(pointsCount, allocator);
            OpenedList = new(pointsCount - 1, allocator);
            NavigationData = new( (int)math.ceil(pointsCount * 0.8f), allocator);
            ResultingData = new(pointsCount, allocator);
            PathCost = new(allocator);
        }

        public void Reset()
        {
            var navData = NavigationData;
            for (var i = 0; i < navData.Length; i++)
            {
                var data = navData[i];
                navData[i] = new(data.NodeKey);
            }

            OpenedList.Clear();
            ResultingData.Clear();
        }

        public void Dispose()
        {
            HexData.Dispose();
            OpenedList.Dispose();
            NavigationData.Dispose();
            ResultingData.Dispose();
            PathCost.Dispose();
        }

        public float GetPathCost(int2 hexCoord, HexEdge edge) => NavigationData[HexData[hexCoord].GetNodeIndex((int)edge)].NodeCost;
        public bool IsEdgeAccessible(int2 hexCoord, HexEdge startEdge, HexEdge endEng) => HexData[hexCoord].AccessMap.IsEdgeAccessible(startEdge, endEng);
    }
}
