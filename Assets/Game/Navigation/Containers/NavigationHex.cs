using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public interface INavigationHex
    {
        float2 CenterPosWorld {get; }
        int2 HexCoordinate { get;}
        IFlowMap FlowMap { get;}
        public NavigationHexPosition Pos { get;}
    }

    public class NavigationHex : INavigationHex, IDisposable
    {
        public int Version { get; private set; } = 0;
        public IFlowMap FlowMap => _flowMap;
        public IntTriangularPos TriangularCenterPos => _pos.TriangularCenterPos;
        public IntTriangularPos InnerRingTopTrianglePos => _pos.InnerRingTopValleyTriangle;
        public float3 CenterPos3DWorld => _pos.CenterPos3DWorld;
        public float2 CenterPosWorld => _pos.CenterPosWorld;

        public int2 HexCoordinate => _pos.HexCoordinate;

        public NavigationHexPosition Pos => _pos;
        private IDisposableFlowMap _flowMap;
        private readonly NavigationHexPosition _pos;


        public NavigationHex(in NavigationHexPosition data)
        {
            _pos = data;
        }

        public void UpdateFlowMap(IDisposableFlowMap flowMap)
        {
            _flowMap?.Dispose();
            _flowMap = flowMap;
            Version++;
        }

        public void Dispose()
        {
            _flowMap?.Dispose();
        }
    }
}
