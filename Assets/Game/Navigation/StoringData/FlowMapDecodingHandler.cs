using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation.DataStoring
{
    public static class FlowMapDecodingHandler
    {

        // note: it is possible to read \ write native collection directly from file
        public static StoredHexData Encode(INavigationMap navMap, IFlowMap flowMap, NavigationHexPosition hexPos)
        {
            switch (flowMap.Type)
            {
                case FlowMapType.Calculated:
                    {
                        var allocator = Allocator.TempJob;

                        var hexFlowMap = flowMap as HexFlowMap;
                        using var kvpArray = hexFlowMap.Data.GetKeyValueArrays(allocator);
                        var arrayLength = kvpArray.Length;
                        var recordLength = EncodeFlowMapDataJob.SLICE_LENGTH;
                        using var resultArray = new NativeArray<byte>(recordLength * arrayLength, allocator, NativeArrayOptions.UninitializedMemory);

                        var job = new EncodeFlowMapDataJob()
                        {
                            Keys = kvpArray.Keys,
                            Values = kvpArray.Values,
                            Result = resultArray,
                        };
                        var handle = job.Schedule(arrayLength, 64);
                        handle.Complete();

                        var byteArray = resultArray.ToArray();

                        return new StoredHexData(byteArray, FlowMapType.Calculated, flowMap.GetAccessMap());
                    }
                case FlowMapType.Virtual:
                    {
                        return new (null, FlowMapType.Virtual, flowMap.GetAccessMap(), (flowMap as VirtualFlowMap).DefaultPassability);
                    }
                    case FlowMapType.VirtualWithRealHeights:
                    {
                        var trianglesCount = TriangularMath.GetTrianglesCountInHex(navMap.TrianglesPerHexEdge);
                        var bytes = new byte[trianglesCount * 2];
                        var offset = 0;
                        foreach (var triangle in new HexTrianglesEnumerator(hexPos, navMap.TrianglesPerHexEdge))
                        {
                            var height = flowMap.GetHeight(triangle);
                            //Debug.Log($"{triangle} : {height}");
                            ByteCoder.WriteShortToBufferLittleEndian(bytes, height, offset);
                            offset += 2;
                        }
                        return new (bytes,FlowMapType.VirtualWithRealHeights, flowMap.GetAccessMap(), (flowMap as VirtualFlowMap).DefaultPassability);
                    }
            }
            return default;
        }
    

        public static IFlowMap Decode(StoredHexData data, NavigationHexPosition hexPos, INavigationMap navMap)
        {
            switch (data.MapType)
            {
                case FlowMapType.Calculated:
                    {
                        using var sourceData = new NativeArray<byte>(data.Data, Allocator.TempJob);
                        var trianglesPerHex = TriangularMath.GetTrianglesCountInHex(navMap.TrianglesPerHexEdge);
                        var resultMap = new NativeHashMap<IntTriangularPos, FlowMapCombinedCell>(trianglesPerHex, Allocator.Persistent);

                        var job = new DecodeFlowMapDataJob()
                        {
                            SourceData = sourceData,
                            ResultMap = resultMap,
                        };
                        var handle = job.Schedule();
                        handle.Complete();

                        return new HexFlowMap(resultMap, data.EdgesAccessMap);
                    }
                 case FlowMapType.VirtualWithRealHeights:
                    {
                        var trianglesCount = TriangularMath.GetTrianglesCountInHex(navMap.TrianglesPerHexEdge);
                        var heights = new Dictionary<IntTriangularPos, short>(trianglesCount);
                        var offset = 0;
                        foreach (var triangle in new HexTrianglesEnumerator(hexPos, navMap.TrianglesPerHexEdge))
                        {
                            var height = ByteCoder.ReadShortFromBufferLittleEndian(byteArray: data.Data, offset);
                            //Debug.Log($"{triangle} : {height}");
                            heights.Add(triangle, height);
                            offset += 2;
                        }
                        return new VirtualFlowMapWithHeights(navMap, data.EdgesAccessMap, data.DefaultPassability, heights);
                    }
                 case FlowMapType.Virtual:
                    {
                        return new VirtualFlowMap(navMap, data.EdgesAccessMap, data.DefaultPassability);
                    }
            }
            return null;
        }
    }
}
