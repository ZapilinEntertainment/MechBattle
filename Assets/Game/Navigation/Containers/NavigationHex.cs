using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Navigation
{
    public interface INavigationHex
    {
        int PassabilityVersion { get; }
        float2 CenterPosWorld { get; }
        int2 HexCoordinate { get; }
        NavigationHexPosition Pos { get; }
        IReadOnlyList<int> PortalExitIds { get; }
    }

    public interface IUpdatableNavigationHex : INavigationHex
    {
        void UpdatePassabilityVersion();
        List<int> Exits { get; }
    }


    public class NavigationHex : IUpdatableNavigationHex
    {
        public int PassabilityVersion { get; private set; } = 0;
        public IntTriangularPos TriangularCenterPos => _pos.TriangularCenterPos;
        public IntTriangularPos InnerRingTopTrianglePos => _pos.InnerRingTopValleyTriangle;
        public float3 CenterPos3DWorld => _pos.CenterPos3DWorld;
        public float2 CenterPosWorld => _pos.CenterPosWorld;

        public int2 HexCoordinate => _pos.HexCoordinate;
        public NavigationHexPosition Pos => _pos;
        public IReadOnlyList<int> PortalExitIds => _exits;
        public List<int> Exits => _exits;

        private readonly NavigationHexPosition _pos;
        private readonly List<int> _exits = new();



        public NavigationHex(in NavigationHexPosition pos)
        {
            _pos = pos;
        }
        public void UpdatePassabilityVersion() => PassabilityVersion++;
    }
}
