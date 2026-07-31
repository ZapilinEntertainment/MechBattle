using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using TriInspector;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Develop
{
    public class HeightDrawer : MonoBehaviour
    {
        [SerializeField] private int2 _hexCoord;
        private INavigationMap _navigationMap;
        private List<(float3 start, float3 end)> _points = new();
        private readonly quaternion rotationRight = Quaternion.AngleAxis(30f, Vector3.up);
        private readonly quaternion rotationLeft = Quaternion.AngleAxis(30f, Vector3.down);

        [Inject]
        public void Inject(INavigationMap map)
        {
            _navigationMap = map;
        }

        [Button, EnableInPlayMode]
        private void Redraw()
        {
            if (_navigationMap.ContainsHex(_hexCoord))
                DrawHeightMap(_hexCoord);
            else
                UnityEngine.Debug.LogWarning("hex not exists");
        }

        private void DrawHeightMap(int2 hexCoord)
        {
            _points.Clear();
            var triangleHeight = _navigationMap.TriangleHeight;

            var hexPos = new NavigationHexPosition(hexCoord, _navigationMap);
            foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, _navigationMap.TrianglesPerHexEdge))
            {
                var vertices = GetTriangleVerticesCommand.Execute(tripos, triangleHeight, _navigationMap.GetHeightData(tripos));
                vertices.AddPointsToList(_points);
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
