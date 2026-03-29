using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class FlowMapCellsDrawWizard : ScriptableWizard
    {
        [Serializable]
        public struct FlowMapCellDrawData
        {
            public Vector3Int Pos;
            public PeakNeighbour PeakDir;
            public ValleyNeighbour ValleyDir;

            public IntTriangularPos Tripos => new IntTriangularPos(Pos.x, Pos.y, Pos.z);
            public byte Direction => Tripos.IsPeak ? (byte)PeakDir : (byte)ValleyDir;
        }

        public FlowMapCellDrawData[] DrawData;

        private bool _mapSettingsPresented = false;
        private List<(float3 start, float3 end)> _points = new();

        private readonly quaternion rotationRight = Quaternion.AngleAxis(30f, Vector3.up);
        private readonly quaternion rotationLeft = Quaternion.AngleAxis(30f, Vector3.down);

        [MenuItem("ZE.Navigation/Draw Flow Map Cells")]
        static void OpenWizard()
        {
            DisplayWizard<FlowMapCellsDrawWizard>("Flow Map Cells Wizard", "Close");
        }

        void OnWizardUpdate()
        {
            if (DrawData == null) return;

            var mapSettings = NavigationDebugDataContainer.MapSettings;
            _mapSettingsPresented = mapSettings != null;
            errorString = _mapSettingsPresented ? string.Empty : "No map settings found";


            _points.Clear();
            //draw:
            var triangleHeight = mapSettings.TriangleHeight;
            var arrowSize = 0.3f * triangleHeight;
            foreach (var drawData in DrawData)
            {
                // direction arrow:
                var tripos = drawData.Tripos;
                var worldPos = TriangularMath.TriangularToWorld(tripos, triangleHeight);
                var vector = TriangularMath.TriangularDirectionToWorld(drawData.Direction, tripos.IsPeak);

                var endPos = arrowSize * vector + worldPos;
                _points.Add((worldPos, endPos));
                _points.Add((endPos, 0.3f * arrowSize * math.mul(rotationRight, -vector) + endPos));
                _points.Add((endPos, 0.3f * arrowSize * math.mul(rotationLeft, -vector) + endPos));

                // triangle border:
                var vertices = GetTriangleVerticesCommand.Execute(tripos, triangleHeight);
                _points.Add((vertices.A, vertices.B));
                _points.Add((vertices.B, vertices.C));
                _points.Add((vertices.A, vertices.C));
            }

            SceneView.RepaintAll();
        }

        void OnWizardCreate()
        {
            
        }


        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (!_mapSettingsPresented)
                return;

            foreach (var pts in _points)
            {
                Handles.DrawLine(pts.start, pts.end);
            }
        }
    }
}

