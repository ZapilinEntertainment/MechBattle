using System;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class HexPathJobCollections : IDisposable
    {
        public NativeHashMap<int2, HexEdgeNodesData> HexData;
        public NativeHashSet<int> OpenedList;
        public NativeArray<AstarPathNodeData<HexEdgeKey>> NavigationData;
        public NativeList<HexEdgeKey> ResultingData;
        public NativeReference<float> PathCost;

        public HexPathJobCollections(Allocator allocator, int hexesCount)
        {
            var pointsCount = hexesCount * 6;
            HexData = new(pointsCount, allocator);
            OpenedList = new(pointsCount - 1, allocator);
            NavigationData = new( pointsCount, allocator);
            ResultingData = new(pointsCount, allocator);
            PathCost = new(allocator);
        }

        public void Dispose()
        {
            if (HexData.IsCreated) HexData.Dispose();
            if (OpenedList.IsCreated) OpenedList.Dispose();
            if (NavigationData.IsCreated) NavigationData.Dispose();
            if (ResultingData.IsCreated) ResultingData.Dispose();
            if (PathCost.IsCreated) PathCost.Dispose();
        }

        public float GetPathCost(int2 hexCoord, HexEdge edge) => NavigationData[HexData[hexCoord].GetNodeIndex((int)edge)].TotalPathCost;
        public bool IsEdgeAccessible(int2 hexCoord, HexEdge startEdge, HexEdge endEng) => HexData[hexCoord].AccessMap.AreEdgesConnected(startEdge, endEng);
    }
}
