using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using TriInspector;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Develop
{
    public class FlowMapDrawer : MonoBehaviour
    {
        [SerializeField] private int _flowMapId;
        private IFlowMapsList _flowMapsList;
        private INavigationMap _navigationMap;
        private List<(float3 start, float3 end)> _points = new();
        private readonly quaternion rotationRight = Quaternion.AngleAxis(30f, Vector3.up);
        private readonly quaternion rotationLeft = Quaternion.AngleAxis(30f, Vector3.down);

        [Inject]
        public void Inject(IFlowMapsList flowMaps, INavigationMap map)
        {
            _flowMapsList = flowMaps;
            _navigationMap = map;
        }

        [Button("Redraw"), EnableInPlayMode]
        private void Redraw()
        {
            if (_flowMapsList.TryGetPathById(_flowMapId, out var flowMap))
            {
                DrawFlowMap(flowMap);
            }
            else
            {
                _flowMapId = -1;
            }
        }

        private void DrawFlowMap(PortalExitFlowMap flowMap)
        {
            _points.Clear();
            var triangleHeight = _navigationMap.TriangleHeight;
            var arrowSize = 0.3f * triangleHeight;

            var hexPos = new NavigationHexPosition(flowMap.HexCoord, _navigationMap);
            foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, _navigationMap.TrianglesPerHexEdge))
            {
                var direction = flowMap.GetDirectionUnsafe(tripos);

                // direction arrow:
                var worldPos = TriangularMath.TriangularToWorld(tripos, triangleHeight);
                var vector = TriangularMath.TriangularDirectionToWorld((byte)direction, tripos.IsPeak);

                var endPos = arrowSize * vector + worldPos;
                _points.Add((worldPos, endPos));
                _points.Add((endPos, 0.3f * arrowSize * math.mul(rotationRight, -vector) + endPos));
                _points.Add((endPos, 0.3f * arrowSize * math.mul(rotationLeft, -vector) + endPos));

                //// triangle border:
                //var vertices = GetTriangleVerticesCommand.Execute(tripos, triangleHeight);
                //vertices.AddPointsToList(_points);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabled) return;
            foreach (var pts in _points)
            {
                Handles.DrawLine(pts.start, pts.end);
            }
        }

    }
}
