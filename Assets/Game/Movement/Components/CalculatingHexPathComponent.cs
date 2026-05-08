using Scellecs.Morpeh;
using UnityEngine;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs 
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct CalculatingHexPathComponent : IComponent 
    {
        public readonly int2 StartHex;
        public readonly int2 EndHex;
        public readonly HexEdgesMask StartEdgesMask;
        public readonly HexEdgesMask EndEdgesMask;
        public readonly int UsedPathListVersion;

        public CalculatingHexPathComponent(int2 startHex, HexEdgesMask startMask, int2 endHex, HexEdgesMask endMask, int pathListVersion)
        {
            StartHex = startHex;
            EndHex = endHex;
            StartEdgesMask = startMask;
            EndEdgesMask = endMask;
            UsedPathListVersion = pathListVersion;
        }

        public CalculatingHexPathComponent(in HexPathSearchResultData data)
        {
            StartHex = data.StartHex;
            EndHex = data.EndHex;
            StartEdgesMask = data.StartEdgesMask;
            EndEdgesMask = data.EndEdgesMask;
            UsedPathListVersion = data.ListVersion;
        }
    }
}