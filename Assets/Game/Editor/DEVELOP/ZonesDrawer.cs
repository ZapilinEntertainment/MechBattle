using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using UnityEngine.Rendering;
using VContainer;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Navigation.DebugOverlay;
using TriInspector;

namespace ZE.MechBattle.Develop
{
    public class ZonesDrawer : MonoBehaviour
    {
        [SerializeField] private List<int2> _hexCoords = new List<int2>() { int2.zero};
        [SerializeField] private CompareFunction _compareFunction = CompareFunction.LessEqual;
        private INavigationMap _map; 
        private List<(Color color, int zone, TriangleVertices vertices, Vector3 worldPos)> _drawData = new();
        private static readonly Color _startColor = Color.darkGreen;
        private static readonly Color _endColor = Color.blue;

        [Inject]
        public void Inject(INavigationMap map)
        {
            _map = map;
        }

        
        [Button("Redraw")]
        private void RedrawMap()
        {
            _drawData.Clear();
            var zonesList = new HashSet<int>(32);
            foreach (var hex in _map.Hexes)
            {
                if (hex.PassabilityVersion == 0 || !_hexCoords.Contains(hex.HexCoordinate))
                    continue;

                var center = new NavigationHexPosition(hex.HexCoordinate, _map).TriangularCenterPos;
                var enumerator = new HexTrianglesEnumerator(center, _map.TrianglesPerHexEdge);                
                foreach (var tripos in enumerator) 
                { 
                    zonesList.Add(_map.GetPassabilityData(tripos).ZoneIndex);
                }

                var step = 1f / zonesList.Count;              
                enumerator.Reset();

                foreach (var tripos in enumerator)
                {
                    var zoneIndex = _map.GetPassabilityData(tripos).ZoneIndex;
                    var worldPos = TriangularMath.TriangularToWorld(tripos, _map.TriangleHeight);
                    worldPos.y = _map.GetHeightData(tripos).AverageHeight;

                    _drawData.Add((
                        Color.Lerp(_startColor, _endColor, zoneIndex * step), 
                        zoneIndex, 
                        TrianglesDrawHelper.GetDrawVertices(tripos, _map),
                       worldPos));
                }
            }

            zonesList.Clear();
        }
        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            var prevHeight = TrianglesDrawHelper.SwitchZTestAndSave(_compareFunction);
            foreach (var drawData in _drawData)
            {
                Handles.color = drawData.color;
                TrianglesDrawHelper.DrawHandles(drawData.vertices, true);
                Handles.Label(drawData.worldPos, drawData.zone.ToString());
            }
            TrianglesDrawHelper.RestoreZTest(prevHeight);
        }
    }
}
