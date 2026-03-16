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

        public HexPathJobCollections(Allocator allocator, int hexesCount)
        {
            HexData = new(hexesCount, allocator);
            OpenedList = new(hexesCount - 1, allocator);
            NavigationData = new( (int)math.ceil(hexesCount / 6 * 5), allocator);
            ResultingData = new(hexesCount, allocator);
        }

        public void Reset()
        {
            var navData = NavigationData;
            for (var i = 0; i < navData.Length; i++)
            {
                var data = navData[i];
                navData[i] = new(data.NodeKey, data.HeuristicCost);
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
        }
    }
}
