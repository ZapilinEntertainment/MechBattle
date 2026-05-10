using Scellecs.Morpeh;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    public class TrianglePathsLRUBuffer : ClearableLRUPathsBuffer<Entity, IntTriangularPos>
    {

        protected override bool TryFormPathData(in NativeArray<IntTriangularPos> positions, out PathData<IntTriangularPos> pathData)
        {
            if (positions.Length < 2)
            {
                Debug.LogError($"invalid path with {positions.Length} nodes");
                pathData = null;
                return false;
            }

            pathData = new PathData<IntTriangularPos>(positions);
            return true;
        }
    }
}
