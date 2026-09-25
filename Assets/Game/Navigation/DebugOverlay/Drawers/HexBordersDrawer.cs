using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public readonly struct HexBordersDrawer
    {
        private readonly HexPointsPreset _hexPointsPreset;
        private readonly float _hexEdgeSize;
        private readonly float _triangleHeight;

        public HexBordersDrawer(in MapSettings mapSettings) : this(mapSettings.HexEdgeSize, mapSettings.TriangleHeight) { }

        public HexBordersDrawer(float hexEdgeSize, float triangleHeight)
        {
            _hexEdgeSize = hexEdgeSize;
            _triangleHeight = triangleHeight;
            _hexPointsPreset = new HexPointsPreset(_hexEdgeSize);
        }

        public readonly void DrawHex(int2 hexCoord)
        {
            var centerPos = GetHexCenter(hexCoord);

            void DrawLine(float2 pointA, float2 pointB) => 
                Handles.DrawLine(
                    centerPos + new Vector3(pointA.x, 0f, pointA.y),
                    centerPos + new Vector3(pointB.x, 0f, pointB.y));

            DrawLine(_hexPointsPreset.TopRight, _hexPointsPreset.Right);
            DrawLine(_hexPointsPreset.Right, _hexPointsPreset.BottomRight);
            DrawLine(_hexPointsPreset.BottomRight, _hexPointsPreset.BottomLeft);
            DrawLine(_hexPointsPreset.BottomLeft, _hexPointsPreset.Left);
            DrawLine(_hexPointsPreset.Left, _hexPointsPreset.TopLeft);
            DrawLine(_hexPointsPreset.TopRight, _hexPointsPreset.TopLeft);
        }

        public readonly void WriteHexBorders(int2 hexCoord, List<(Vector3, Vector3)> lines)
        {
            var centerPos = GetHexCenter(hexCoord);

            void AddPoints(float2 pointA, float2 pointB) =>
                lines.Add(
                    (centerPos + new Vector3(pointA.x, 0f, pointA.y),
                    centerPos + new Vector3(pointB.x, 0f, pointB.y)));

            AddPoints(_hexPointsPreset.TopRight, _hexPointsPreset.Right);
            AddPoints(_hexPointsPreset.Right, _hexPointsPreset.BottomRight);
            AddPoints(_hexPointsPreset.BottomRight, _hexPointsPreset.BottomLeft);
            AddPoints(_hexPointsPreset.BottomLeft, _hexPointsPreset.Left);
            AddPoints(_hexPointsPreset.Left, _hexPointsPreset.TopLeft);
            AddPoints(_hexPointsPreset.TopRight, _hexPointsPreset.TopLeft);
        }

        private Vector3 GetHexCenter(int2 hexCoord) => (Vector3)new NavigationHexPosition(hexCoord.x, hexCoord.y, _hexEdgeSize, _triangleHeight)
                .CenterPos3DWorld;

    }
}
