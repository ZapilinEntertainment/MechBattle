using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct FlowTrianglePathComponent : IPathUserComponent<int> 
    {
        public readonly int FlowMapId;
        public readonly int2 MapHexCoord;

        public int PathKey => FlowMapId;

        public FlowTrianglePathComponent(int id, int2 hexCoord)
        {
            FlowMapId = id;
            MapHexCoord = hexCoord;
        }
    }
}