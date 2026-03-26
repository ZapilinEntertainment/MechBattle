using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class HexTrianglesDrawWizard : ScriptableWizard
    {
        public int2 HexCoord;
        public int HexRadius;
        private bool _mapSettingsPresented = false;
        private List<TriangleVertices> _vertices = new();

        [MenuItem("ZE.Navigation/Draw Hex Triangles")]
        static void OpenWizard()
        {
            DisplayWizard<HexTrianglesDrawWizard>("Hex Triangles Display", "Close");
        }

        void OnWizardUpdate()
        {
            var mapSettings = NavigationDebugDataContainer.MapSettings;
            _mapSettingsPresented = mapSettings != null;
            errorString = _mapSettingsPresented ? string.Empty : "No map settings found";

            _vertices.Clear();
            if (HexRadius == 0)
                return;

            var mapData = mapSettings.ToStruct();
            using (var list = new NativeArray<IntTriangularPos>(TriangularMath.GetTrianglesCountInHex(HexRadius), Allocator.TempJob))
            {
                var navHex = new NavigationHexPosition(HexCoord.x, HexCoord.y, mapData.HexEdgeSize, mapData.TriangleHeight);
                GetTrianglesInHexCommand.Execute(navHex.InnerRingTopTriangle, HexRadius, list);

                foreach (var triangle in list)
                {
                    _vertices.Add(GetTriangleVerticesCommand.Execute(triangle, mapData.TriangleHeight));                    
                }
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
            
            foreach (var triangleVertices in _vertices)
            {
                Handles.DrawLine(triangleVertices.A, triangleVertices.B);
                Handles.DrawLine(triangleVertices.B, triangleVertices.C);
                Handles.DrawLine(triangleVertices.A, triangleVertices.C);
            }
        }
    }

}
