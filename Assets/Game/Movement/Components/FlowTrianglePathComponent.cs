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
    public readonly struct FlowTrianglePathComponent : IComponent 
    {
        public readonly HexEdge ExitEdge;
        public readonly int2 NextHexCoord;

        public FlowTrianglePathComponent(HexEdge flowMapExitEdge, int2 nextHexCoord)
        {
            ExitEdge = flowMapExitEdge;
            NextHexCoord = nextHexCoord;
        }
    }
}