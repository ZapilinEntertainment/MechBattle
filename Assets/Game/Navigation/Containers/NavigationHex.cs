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
        float2 CenterPosWorld { get; }
        int2 HexCoordinate { get; }
        HexEdgesAccessMap AccessMap { get; }
        HexEdgesMask EdgesPassability { get;}
        NavigationHexPosition Pos { get; }
        IReadOnlyList<NavigationPortal> PortalsList { get; }
    }

    public interface IUpdatableNavigationHex : INavigationHex
    {
        void UpdateAccessMap(HexEdgesAccessMap map);
        void UpdateEdgesPassability(HexEdgesMask mask);
        void UpdateVersion();
    }

    public class NavigationHex : IUpdatableNavigationHex
    {
        public int Version { get; private set; } = 0;
        public IntTriangularPos TriangularCenterPos => _pos.TriangularCenterPos;
        public IntTriangularPos InnerRingTopTrianglePos => _pos.InnerRingTopValleyTriangle;
        public float3 CenterPos3DWorld => _pos.CenterPos3DWorld;
        public float2 CenterPosWorld => _pos.CenterPosWorld;

        public int2 HexCoordinate => _pos.HexCoordinate;

        public NavigationHexPosition Pos => _pos;
        public HexEdgesAccessMap AccessMap { get;private set;}
        public HexEdgesMask EdgesPassability { get;private set;}
        public IReadOnlyList<NavigationPortal> PortalsList => _portals;

        private readonly NavigationHexPosition _pos;
        private readonly List<NavigationPortal> _portals = new();



        public NavigationHex(in NavigationHexPosition pos)
        {
            _pos = pos;
        }

        public void UpdateEdgesPassability(HexEdgesMask mask) => EdgesPassability = mask;
        public void UpdateAccessMap(HexEdgesAccessMap accessMap) => AccessMap = accessMap;  
        public void UpdateVersion() => Version++;
    }
}
