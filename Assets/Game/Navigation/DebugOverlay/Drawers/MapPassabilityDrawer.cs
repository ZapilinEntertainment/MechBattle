using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class MapPassabilityDrawer
    {
        [SerializeField] private CompareFunction _compareFunction = CompareFunction.LessEqual;
        private readonly List<TriangleDrawData> _drawData = new();
        private const float ALPHA = 0.5f;
        private readonly Color _passableColor = new Color(0f, 0.1f, 1f, ALPHA);
        private readonly Color _impassableColor = new Color(1f, 0.1f, 0f, ALPHA);

        public void RedrawMap(INavigationMap map)
        {
            _drawData.Clear();
            foreach (var hex in map.Hexes)
            {
                if (hex.PassabilityVersion == 0)
                    continue;
                TrianglesDrawHelper.AddHexTrianglesData(hex.HexCoordinate, map, _drawData);
            }
        }

        public void DrawHandles()
        {
            if (_drawData.Count == 0)
                return;

            var prevZTest = TrianglesDrawHelper.SwitchZTestAndSave(_compareFunction);
            //var isPassable = false; 
            Handles.color = _impassableColor;
            foreach (var triangleData in _drawData)
            {
                if (triangleData.IsPassable)
                    continue;
                //if (triangleData.IsPassable != isPassable)
                //{
                //    isPassable = triangleData.IsPassable;
                //    Handles.color = isPassable ? _passableColor : _passableColor;
                //}
                TrianglesDrawHelper.DrawHandles(triangleData, false);
            }
            TrianglesDrawHelper.RestoreZTest(prevZTest);
        }
    }
}
