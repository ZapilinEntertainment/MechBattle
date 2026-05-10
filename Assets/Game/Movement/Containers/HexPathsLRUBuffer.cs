using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Scellecs.Morpeh;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public class HexPathsLRUBuffer : ClearableLRUPathsBuffer<Entity, HexPathNodeKey>
    {

        protected override bool TryFormPathData(in NativeArray<HexPathNodeKey> positions, out PathData<HexPathNodeKey> pathData)
        {
            if (positions.Length < 2)
            {
                Debug.LogError($"invalid hex path with {positions.Length} nodes");
                pathData = default;
                return false;
            }

            pathData = new PathData<HexPathNodeKey>(positions);
            return true;
        }
    }
}
