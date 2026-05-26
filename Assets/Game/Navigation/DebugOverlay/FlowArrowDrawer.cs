using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class FlowArrowDrawer
    {
        private readonly INavigationMap _map;
        private readonly float _arrowSize;
        private readonly float _triangleHeight;
        private readonly quaternion rotationRight = Quaternion.AngleAxis(30f, Vector3.up);
        private readonly quaternion rotationLeft = Quaternion.AngleAxis(30f, Vector3.down);

        private List<(float3 start, float3 end)> _points = new();

        public FlowArrowDrawer(INavigationMap map, float arrowSize)
        {
            _map = map;
            _arrowSize = arrowSize;

            _triangleHeight = map.TriangleHeight;
        }

        public void DrawFlowArrow(IntTriangularPos pos, byte direction)
        {
            var vector = TriangularMath.TriangularDirectionToWorld(direction, pos.IsPeak);
            var heightData = _map.GetHeightData(pos);

            var worldPos = TriangularMath.TriangularToWorld(pos, _triangleHeight);
            worldPos.y = heightData[(int)TriangleHeightMeasurePoint.Average];

            var endPos = _arrowSize * vector + worldPos;
            _points.Add((worldPos, endPos));
            _points.Add((endPos, 0.3f * _arrowSize * math.mul(rotationRight, -vector) + endPos));
            _points.Add((endPos, 0.3f * _arrowSize * math.mul(rotationLeft, -vector) + endPos));
        }

        public void Clear() => _points.Clear();

        public void OnSceneGUI()
        {
            Handles.color = Color.white;
            foreach (var pts in _points)
            {
                Handles.DrawLine(pts.start, pts.end);
            }
        }

    }
}
