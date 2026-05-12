using Scellecs.Morpeh;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class TrianglePathsLRUBuffer : UserCountDependentLRUPathsBuffer<Entity, IntTriangularPos>
    {

        protected override bool TryFormPathData(CalculatedPathData<IntTriangularPos> positions, out PathData<IntTriangularPos> pathData)
        {
            var length = positions.Points.Length;
            if (length == 0)
            {
                Debug.LogError("invalid path with zero nodes");
                pathData = null;
                return false;
            }

            pathData = new PathData<IntTriangularPos>(positions.Points, positions.PathCost);
            return true;
        }
    }
}
