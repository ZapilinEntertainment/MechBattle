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
    }

    public class NavigationHex : INavigationHex, IDisposable
    {
        public int Version { get; private set; } = 0;
        public HexFlowMap FlowMap { get; private set; }
        public IntTriangularPos TriangularCenterPos => Data.TriangularCenterPos;
        public IntTriangularPos InnerRingTopTrianglePos => Data.InnerRingTopTriangle;
        public float3 CenterPos3DWorld => Data.CenterPos3D;
        public float2 CenterPosWorld => Data.CenterPos;

        public int2 HexCoordinate => Data.HexCoordinate;

        public readonly NavigationHexPosition Data;      

        public SquaredHexTrianglesList<NavigationTriangleData> TrianglesData;


        public NavigationHex(in NavigationHexPosition data)
        {
            Data = data;
        }

        public void UpdateFlowMap(HexFlowMap flowMap)
        {
            FlowMap?.Dispose();
            FlowMap = flowMap;
            Version++;
        }

        public void UpdateTrianglesData(in SquaredHexTrianglesList<NavigationTriangleData> data)
        {
            TrianglesData = data;
            Version++;
        }

        public void Dispose()
        {
            FlowMap?.Dispose();
        }
    }
}
