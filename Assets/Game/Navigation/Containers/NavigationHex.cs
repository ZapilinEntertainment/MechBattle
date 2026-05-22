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
        bool IsFlowMapCalculated { get; }
        float2 CenterPosWorld { get; }
        int2 HexCoordinate { get; }
        HexEdgesAccessMap AccessMap { get; }
        HexEdgesMask EdgesPassability { get;}
        NavigationHexPosition Pos { get; }
        HexEdgeStatus GetEdgeStatus(HexEdge edge);
        IReadOnlyList<NavigationPortal> PortalsList { get; }
    }

    public interface IUpdatableNavigationHex : INavigationHex
    {
        void UpdateAccessMap(HexEdgesAccessMap map);
        void UpdateEdgesPassability(HexEdgesMask mask);
        void UpdateVersion();
        void OnFlowMapCalculated();
    }

    public class NavigationHex : IUpdatableNavigationHex
    {
        public bool IsFlowMapCalculated { get; private set;}
        public int Version { get; private set; } = 0;
        public IntTriangularPos TriangularCenterPos => _pos.TriangularCenterPos;
        public IntTriangularPos InnerRingTopTrianglePos => _pos.InnerRingTopValleyTriangle;
        public float3 CenterPos3DWorld => _pos.CenterPos3DWorld;
        public float2 CenterPosWorld => _pos.CenterPosWorld;

        public int2 HexCoordinate => _pos.HexCoordinate;

        public NavigationHexPosition Pos => _pos;
        public HexEdgesAccessMap AccessMap { get;private set;}
        public HexEdgesMask EdgesPassability { get;private set;}
        public IReadOnlyList<NavigationPortalExit> PortalsList => _portals;

        private readonly NavigationHexPosition _pos;
        private readonly HexEdgeStatus[] _edgeStatuses = new HexEdgeStatus[6];
        private readonly List<NavigationPortalExit> _portals = new();



        public NavigationHex(in NavigationHexPosition pos)
        {
            _pos = pos;
        }

        public void UpdateEdgesPassability(HexEdgesMask mask) => EdgesPassability = mask;
        public void UpdateAccessMap(HexEdgesAccessMap accessMap) => AccessMap = accessMap;
        public void OnFlowMapCalculated() => IsFlowMapCalculated = true;    
        public void UpdateVersion() => Version++;
        public HexEdgeStatus GetEdgeStatus(HexEdge edge) => _edgeStatuses[(int)edge];
    }
}
