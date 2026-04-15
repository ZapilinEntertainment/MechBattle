using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class TriangleSubdivisionsScriptableWizard : ScriptableWizard
    {
        public float TriangleEdgeSize = 25f;
        public int3 TrianglePos = new(0,1,0);
        public int TrianglesPerEdge = 4;

        private List<TriangleVertices> _vertices = new();
        private readonly float RAD_CF = math.sqrt(3) / 6f;

        [MenuItem("ZE.Navigation/Draw Triangle Subdivision")]
        static void OpenWizard()
        {
            DisplayWizard<TriangleSubdivisionsScriptableWizard>("Triangle Subdivision Display", "Close");
        }

        void OnWizardUpdate() 
        {
            _vertices.Clear();

            var tripos = new IntTriangularPos(TrianglePos);
            var triangleHeight = TriangleEdgeSize * NavigationConstants.SQRT_OF_THREE_HALVED;
            _vertices.Add(GetTriangleVerticesCommand.Execute(tripos, triangleHeight, offset: 0f));

            var center = TriangularMath.TriangularToWorld(tripos, triangleHeight).xz;
            using var centersArray = SubdivideTriangleCommand.CreateDataArray(TrianglesPerEdge, Allocator.Temp);
            SubdivideTriangleCommand.Execute(center, tripos.IsPeak, new()
            {
                Centers = centersArray,
                RaycastTrianglesPerEdge = TrianglesPerEdge,
                TriangleHeight = triangleHeight
            });

            var edge = TriangleEdgeSize / TrianglesPerEdge;
            for (var i = 0; i < centersArray.Length; i++)
            {
                var data = centersArray[i];
                var vertices = GetTriangleVerticesCommand.Execute(data.WorldPos, data.IsPeak, edge);
                _vertices.Add(vertices);

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
            foreach (var tri in _vertices)
            {
                Handles.DrawLine(tri.A, tri.B);
                Handles.DrawLine(tri.B, tri.C);
                Handles.DrawLine(tri.A, tri.C);
            }
        }
    }
}
