using System;
using UnityEngine;
using TriInspector;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Develop
{
    public class NavigationCellDisplay : MonoBehaviour
    {
        [Serializable]
        public struct DisplayStruct
        {
            public int TopExitDistance;
            public int TopRightDistance;
            public int BottomRightDistance;
            public int BottomDistance;
            public int BottomLeftDistance;
            public int TopLeftDistance;
            [Space]
            public string AccessMask;
        }

        [ReadOnly, SerializeField] private IntTriangularPos _pos;
        [ReadOnly, SerializeField] private DisplayStruct _displayData;
        private INavigationMap _map;
        private IntTriangularPos _lastDisplayedPos;

        [Inject]
        public void Inject(INavigationMap map)
        {
            _map = map;
        }

        private void Update()
        {
            if (_map == null)
                return;

            _pos = TriangularMath.WorldToTrianglePos(transform.position, _map.TriangleHeight);

            if (_lastDisplayedPos != _pos)
            {
                var flowData = _map.GetFlowData(_pos);
                _displayData = new()
                {
                    TopExitDistance = flowData[HexEdge.Top].ExitDistance,
                    TopRightDistance = flowData[HexEdge.TopRight].ExitDistance,
                    BottomRightDistance = flowData[HexEdge.BottomRight].ExitDistance,
                    BottomDistance = flowData[HexEdge.Bottom].ExitDistance,
                    BottomLeftDistance = flowData[HexEdge.BottomLeft].ExitDistance,
                    TopLeftDistance = flowData[HexEdge.TopLeft].ExitDistance,

                    AccessMask = flowData.GetCombinedEdgeAccessMask().ToString()
                };
                _lastDisplayedPos = _pos;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_map == null)
                return;
            var vertices = GetTriangleVerticesCommand.Execute(_pos, _map.TriangleHeight, 0f);
            Gizmos.DrawLine(vertices.PinnaclePos, vertices.LeftBasisPos);
            Gizmos.DrawLine(vertices.LeftBasisPos, vertices.RightBasisPos);
            Gizmos.DrawLine(vertices.RightBasisPos, vertices.PinnaclePos);
        }
        #endif
    }
}
