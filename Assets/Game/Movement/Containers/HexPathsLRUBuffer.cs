using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Scellecs.Morpeh;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{

    public class HexPathsLRUBuffer : UserCountDependentLRUPathsBuffer<Entity, HexPathNodeKey>
    {

        protected override bool TryFormPathData(CalculatedPathData<HexPathNodeKey> calculatedData, out PathData<HexPathNodeKey> pathData)
        {
            if (calculatedData.Points.Length < 2)
            {
                Debug.LogError($"invalid hex path with {calculatedData.Points.Length} nodes");
                pathData = default;
                return false;
            }

            pathData = new PathData<HexPathNodeKey>(calculatedData.Points, calculatedData.PathCost);
            return true;
        }
    }
}
