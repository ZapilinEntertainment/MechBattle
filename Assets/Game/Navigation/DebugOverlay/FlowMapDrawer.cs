using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using UnityEditor;


namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class FlowMapDrawer : IDisposable
    {
        private List<(float3 start, float3 end)> _gizmosDrawData = new();
        private float _arrowSize = 1f;
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly quaternion rotationRight = Quaternion.AngleAxis(30f,Vector3.up);
        private readonly quaternion rotationLeft = Quaternion.AngleAxis(30f, Vector3.down);

        public async Awaitable DrawFlowFieldAsync(int2 hexCoord, HexEdge exitEdge)
        {
            var map = NavigationDebugDataContainer.Map;
            if (map == null)
            {
                Debug.LogError("draw map first");
                return;
            }
            var caster = NavigationDebugDataContainer.Caster;
            if (caster == null)
            {
                Debug.LogError("no nav caster found");
                return;
            }

            await UpdateFlowMapAsync(hexCoord, exitEdge, map, caster);
        }

        public void OnSceneGUI()
        {
            if (_gizmosDrawData.Count != 0)
            {
                foreach (var data in _gizmosDrawData)
                {
                    Handles.DrawLine(data.start, data.end);
                }
            }
        }

        private async Awaitable UpdateFlowMapAsync(int2 hexCoord, HexEdge exitEdge, INavigationMap map, INavigationCaster caster)
        {
            _gizmosDrawData.Clear();
            var flowMap = map.GetFlowMap(hexCoord);
            HexFlowMap castedFlowMap;
            if (flowMap.IsStub)
            {
                var token = _tokenSource.Token;
                castedFlowMap = await CalculateHexFlowMapCommand.ExecuteAsync(map.GetHexData(hexCoord), caster, token);
                if (token.IsCancellationRequested || map == null)
                {
                    castedFlowMap.Dispose();
                    return;
                }

                if (!map.TryGetHex(hexCoord, out var hex))
                    map.AddHex(hexCoord);

                map.UpdateHexFlowMap(hexCoord, castedFlowMap);
            }
            else
            {
                castedFlowMap = flowMap as HexFlowMap;
            }

            //draw:
            _arrowSize = 0.3f * caster.TriangleEdgeSize;
            foreach (var kvp in castedFlowMap.Data)
            {
                // direction arrow:
                var worldPos = TriangularMath.TriangularToWorld(kvp.Key, map.TriangleEdgeSize);
                var flowMapCell = kvp.Value[exitEdge];
                var vector = TriangularMath.TriangularDirectionToWorld(flowMapCell.Direction, kvp.Key.IsPeak);

                var endPos = _arrowSize * vector + worldPos;
                _gizmosDrawData.Add((worldPos, endPos));
                _gizmosDrawData.Add((endPos, 0.3f * _arrowSize * math.mul(rotationRight, -vector) + endPos));
                _gizmosDrawData.Add((endPos, 0.3f * _arrowSize * math.mul(rotationLeft, -vector) + endPos));

                // triangle border:
                var vertices = NavigationMapHelper.GetTriangleVertices(kvp.Key, caster.TriangleEdgeSize);
                _gizmosDrawData.Add((vertices.A, vertices.B));
                _gizmosDrawData.Add((vertices.B, vertices.C));
                _gizmosDrawData.Add((vertices.A, vertices.C));
            }


            using var trianglesList = new NativeArray<IntTriangularPos>(caster.HexTrianglesCount, Allocator.TempJob);
            var hexPos = new NavigationHexPosition(hexCoord.x, hexCoord.y, map.HexEdgeSize, map.TriangleEdgeSize);
            GetTrianglesInHexCommand.Execute(hexPos.InnerRingTopTriangle, map.TrianglesPerHexEdge, trianglesList);
            foreach (var triPos in trianglesList)
            {
                if (!castedFlowMap.Data.ContainsKey(triPos)) 
                    Debug.Log($"triangle not found: {triPos}");
            }
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
    }
}
