using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    [ExecuteInEditMode]
    public class NavigationMapDrawer : MonoBehaviour
    {
        [SerializeField] private float _hexEdgeSize = 10f;
        [SerializeField] private Vector2 _bottomLeftCorner;
        [SerializeField] private Vector2 _topRightCorner;

        private NavigatonMap _map;

        public void RedrawMap()
        {
            _map = NavigationMapBuilder.Build(_bottomLeftCorner, _topRightCorner, _hexEdgeSize, 0);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            var point10 = new Vector3(_topRightCorner.x, 0f, _bottomLeftCorner.y);
            var point01 = new Vector3(_bottomLeftCorner.x, 0f, _topRightCorner.y);
            var point00 = new Vector3(_bottomLeftCorner.x, 0f, _bottomLeftCorner.y);
            var point11 = new Vector3(_topRightCorner.x, 0f, _topRightCorner.y);
            Gizmos.DrawLine(point00, point01);
            Gizmos.DrawLine(point00, point10);
            Gizmos.DrawLine(point01, point11);
            Gizmos.DrawLine(point10, point11);

            if (_map == null)
                return;

            Gizmos.color = Color.white;
            var edge = _map.HexEdgeSize;
            var dir = new Vector3(0,0, edge);
            var hexPointsPreset = new Vector3[6];
            for (var i = 0; i < 6; i++)
            {
                hexPointsPreset[i] = Quaternion.AngleAxis(30f + i * 60f, Vector3.up) * dir;
            }

            foreach (var hex in _map.Hexes.Values)
            {
                DrawHex(hex.Center);
            }

            void DrawHex(float2 centerPos)
            {
                var center = new Vector3(centerPos.x, 0f, centerPos.y);
                for (var i = 0; i < 5; i++)
                {
                    Gizmos.DrawLine(center + hexPointsPreset[i], center + hexPointsPreset[i+1]);
                }
                Gizmos.DrawLine(center + hexPointsPreset[5], center + hexPointsPreset[0]);
            }

        }
    }
}
