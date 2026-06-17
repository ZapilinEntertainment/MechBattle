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
#if UNITY_EDITOR
            try
            {
                FinalDispose();
            }
            catch (Exception ex)
            {
                if (!ZE.Utils.EditorPlaymodeLifetimeObject.IsQuitting)
                    UnityEngine.Debug.LogError(ex);
            }
            return;
#else  

            FinalDispose();       
#endif  
        }

        private void FinalDispose()
        {
            HexData.Dispose();
            OpenedList.Dispose();
            NavigationData.Dispose();
            ResultingData.Dispose();
            PathCost.Dispose();
        }

        public float GetPathCost(int2 hexCoord, HexEdge edge) => NavigationData[HexData[hexCoord].GetNodeIndex((int)edge)].TotalPathCost;
        public bool IsEdgeAccessible(int2 hexCoord, HexEdge startEdge, HexEdge endEng) => HexData[hexCoord].AccessMap.AreEdgesConnected(startEdge, endEng);
    }
}
