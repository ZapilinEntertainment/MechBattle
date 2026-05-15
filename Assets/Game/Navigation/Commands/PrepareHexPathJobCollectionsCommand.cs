using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{

    public static class PrepareHexPathJobCollectionsCommand
    {
        public static HexPathJobCollections Execute(Allocator allocator, INavigationMap map)
        {
            var data = new HexPathJobCollections(allocator, map.Hexes.Count);

            var transitionableNodes = GetHexTransitionableNodesCommand.Execute(map, checkEdgesPassability: true);
            var indicesDictionary = new Dictionary<HexPathNodeKey, int>();
            var currentHexEdgeDataIndices = new int[6];
            var nextIndex = 0;
            foreach (var hexPos in map.HexCoords)
            {
                var hex = map.GetOrCreateHex(hexPos);
                var accessMap = hex.AccessMap;
                var edgesPassability = hex.EdgesPassability;

                for (var edgeIndex = 0; edgeIndex < 6; edgeIndex++)
                {
                    var key = new HexPathNodeKey(hexPos, edgeIndex);
                    var oppositeKey = key.ToOpposite();

                    if (!transitionableNodes.IsNodeTransitionable(key) )
                    {
                        currentHexEdgeDataIndices[edgeIndex] = HexEdgeNodesData.INVALID_INDEX;
                        continue;
                    }

                    if (indicesDictionary.TryGetValue(oppositeKey, out var alreadyAddedIndex))
                    {
                        // already presented by opposite edge
                        currentHexEdgeDataIndices[edgeIndex] = alreadyAddedIndex;
                        continue;
                    }

                    if (!edgesPassability.IsEdgePresented(edgeIndex))
                    {
                        currentHexEdgeDataIndices[edgeIndex] = HexEdgeNodesData.INVALID_INDEX;
                        continue;
                    }

                    var index = nextIndex++;
                    indicesDictionary.Add(key, index);
                    currentHexEdgeDataIndices[edgeIndex] = index;
                    //Debug.Log($"{index} : {hexPos} : {(HexEdge)edgeIndex}");

                    data.NavigationData[index] = new(new(hexPos, edgeIndex));
                }

                data.HexData.Add(hexPos, new(currentHexEdgeDataIndices, accessMap, edgesPassability));                
            }

            return data;
        }

    }
}
