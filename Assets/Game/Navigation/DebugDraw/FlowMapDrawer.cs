using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

#if UNITY_EDITOR
using TriInspector;
#endif


namespace ZE.MechBattle.Navigation.DebugDraw
{
    public class FlowMapDrawer : MonoBehaviour
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

        [SerializeField] private int2 _hexCoordinate;
        [SerializeField, OnValueChanged(nameof(DrawFlowField))] private HexEdge _exitEdge;
        [SerializeField] private NavigationMapDrawer _mapDrawer;
        private List<GizmosData> _gizmosData = new();
        private float ARROW_LENGTH = 0.5f;
        private NavigationCaster _navigationCaster;
        private readonly CancellationTokenSource _tokenSource = new();

#if UNITY_EDITOR

        [Button("Draw flow map in hex")]
        public void DrawFlowField()
        {
            var map = _mapDrawer?.Map;
            if (map == null)
                return;

            UpdateFlowMap(_hexCoordinate, _exitEdge);
        }

        private void OnDrawGizmos()
        {
            if (_gizmosData.Count != 0)
            {
                var rotationRight = Quaternion.AngleAxis(30f,Vector3.up);
                var rotationLeft = Quaternion.AngleAxis(30f, Vector3.down);

                foreach (var data in _gizmosData)
                {
                    var endPos = data.Direction + data.Position;
                    Gizmos.DrawLine(data.Position, endPos);
                    Gizmos.DrawLine(endPos, 0.3f * (rotationRight * -data.Direction) + endPos);
                    Gizmos.DrawLine(endPos, 0.3f * (rotationLeft * -data.Direction) + endPos);
                }
            }
        }
#endif

        private async void UpdateFlowMap(int2 hexCoord, HexEdge exitEdge)
        {
            _gizmosData.Clear();

            var map = _mapDrawer.Map;
            if (!map.TryGetFlowMap(hexCoord, out var flowMap))
            {                
                _navigationCaster ??= new NavigationCaster(map.Settings, Allocator.Persistent);
                flowMap = await CalculateHexFlowMapCommand.ExecuteAsync(map.GetHexData(hexCoord), _navigationCaster, _tokenSource.Token);
                
                if (!map.TryGetHex(hexCoord, out var hex))
                    map.AddHex(hexCoord);

                map.UpdateHexFlowMap(hexCoord, flowMap);
            }

            //draw:
            foreach (var kvp in flowMap.Data)
            {
                var worldPos = TriangularMath.TriangularToWorld(kvp.Key, map.TriangleEdgeSize);
                var direction = kvp.Value[_exitEdge];
                var vector = TriangularMath.TriangularDirectionToWorld(direction, kvp.Key.IsPeak);
                _gizmosData.Add(new(vector, worldPos));
            }
        }

        private void OnDestroy()
        {
            _navigationCaster.Dispose();
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
    }
}
