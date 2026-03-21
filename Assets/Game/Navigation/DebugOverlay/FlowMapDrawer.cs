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
        private readonly struct GizmosData
        {
            public readonly Vector3 Direction;
            public readonly Vector3 Position;

            public GizmosData(Vector3 dir, Vector3 position)
            {
                Direction = dir;
                Position = position;
            }
        }

        private List<GizmosData> _gizmosData = new();
        private float ARROW_LENGTH = 0.5f;
        private readonly CancellationTokenSource _tokenSource = new();

        public void DrawFlowField(int2 hexCoord, HexEdge exitEdge)
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

            UpdateFlowMap(hexCoord, exitEdge, map, caster);
        }

        public void OnSceneGUI()
        {
            if (_gizmosData.Count != 0)
            {
                var rotationRight = Quaternion.AngleAxis(30f,Vector3.up);
                var rotationLeft = Quaternion.AngleAxis(30f, Vector3.down);

                foreach (var data in _gizmosData)
                {
                    var endPos = data.Direction + data.Position;
                    Handles.DrawLine(data.Position, endPos);
                    Handles.DrawLine(endPos, 0.3f * (rotationRight * -data.Direction) + endPos);
                    Handles.DrawLine(endPos, 0.3f * (rotationLeft * -data.Direction) + endPos);
                }
            }
        }

        private async void UpdateFlowMap(int2 hexCoord, HexEdge exitEdge, INavigationMap map, INavigationCaster caster)
        {
            _gizmosData.Clear();
            var flowMap = map.GetFlowMap(hexCoord);
            HexFlowMap castedFlowMap;
            if (flowMap.IsStub)
            {
                castedFlowMap = await CalculateHexFlowMapCommand.ExecuteAsync(map.GetHexData(hexCoord), caster, _tokenSource.Token);

                if (!map.TryGetHex(hexCoord, out var hex))
                    map.AddHex(hexCoord);

                map.UpdateHexFlowMap(hexCoord, castedFlowMap);
            }
            else
            {
                castedFlowMap = flowMap as HexFlowMap;
            }

            //draw:
            Debug.Log(castedFlowMap.Data.Count);
            foreach (var kvp in castedFlowMap.Data)
            {
                var worldPos = TriangularMath.TriangularToWorld(kvp.Key, map.TriangleEdgeSize);
                var flowMapCell = kvp.Value[exitEdge];
                var vector = TriangularMath.TriangularDirectionToWorld(flowMapCell.Direction, kvp.Key.IsPeak);
                _gizmosData.Add(new(vector, worldPos));
            }
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
    }
}
