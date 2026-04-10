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
    }

    public class NavigationHex : INavigationHex, IDisposable
    {
        public int Version { get; private set; } = 0;
        public IFlowMap FlowMap => _flowMap;
        public IntTriangularPos TriangularCenterPos => Data.TriangularCenterPos;
        public IntTriangularPos InnerRingTopTrianglePos => Data.InnerRingTopValleyTriangle;
        public float3 CenterPos3DWorld => Data.CenterPos3DWorld;
        public float2 CenterPosWorld => Data.CenterPosWorld;

        public int2 HexCoordinate => Data.HexCoordinate;

        public readonly NavigationHexPosition Data;      

        public SquaredHexTrianglesList<TriangleNavData> TrianglesData;
        private IDisposableFlowMap _flowMap;


        public NavigationHex(in NavigationHexPosition data)
        {
            Data = data;
        }

        public void UpdateFlowMap(IDisposableFlowMap flowMap)
        {
            _flowMap?.Dispose();
            _flowMap = flowMap;
            Version++;
        }

        public void UpdateTrianglesData(in SquaredHexTrianglesList<TriangleNavData> data)
        {
            TrianglesData = data;
            Version++;
        }

        public void Dispose()
        {
            _flowMap?.Dispose();
        }
    }
}
