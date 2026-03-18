using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{

    public static class PrepareHexPathJobCollectionsCommand
    {
        public static HexPathJobCollections Execute(Allocator allocator, NavigationMap map)
        {
            var data = new HexPathJobCollections(Allocator.TempJob, map.Hexes.Count);
            var indicesDictionary = new Dictionary<HexPathNodeKey, int>();
            var indicesSendArray = new int[6];
            var nextIndex = 0;
            foreach (var hexPos in map.HexCoords)
            {
                var flowMap = map.GetFlowMap(hexPos);
                var accessMap = flowMap.GetAccessMap();

                for (var edgeIndex = 0; edgeIndex < 6; edgeIndex++)
                {
                    var key = new HexPathNodeKey(hexPos, edgeIndex);

                    if (indicesDictionary.TryGetValue(key.ToOpposite(), out var alreadyAddedIndex))
                    {
                        // already presented by opposite edge
                        indicesSendArray[edgeIndex] = alreadyAddedIndex;
                        continue;
                    }

                    if (!accessMap.IsEdgePassable(edgeIndex))
                    {
                        indicesSendArray[edgeIndex] = HexEdgeNodesData.INVALID_INDEX;
                        continue;
                    }

                    var index = nextIndex++;
                    indicesDictionary.Add(key, edgeIndex);
                    indicesSendArray[edgeIndex] = index;
                    data.NavigationData[index] = new(new(hexPos, edgeIndex));
                }

                data.HexData.Add(hexPos, new(indicesSendArray, accessMap));                
            }

            return data;
        }

    }
}
