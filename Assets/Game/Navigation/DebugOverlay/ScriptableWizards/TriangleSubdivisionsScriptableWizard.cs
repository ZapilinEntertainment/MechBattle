using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class TriangleSubdivisionsScriptableWizard : ScriptableWizard
    {
        public float TriangleEdgeSize = 10f;
        public int3 TrianglePos = new(0,1,0);
        public int TrianglesPerEdge = 4;       

        private float _innerRadius;
        private List<TriangleVertices> _vertices = new();
        private List<Vector3> _centers = new();

        [MenuItem("ZE.Navigation/Draw Triangle Subdivision")]
        static void OpenWizard()
        {
            DisplayWizard<TriangleSubdivisionsScriptableWizard>("Triangle Subdivision Display", "Close");
        }

        void OnWizardUpdate() 
        {
            _vertices.Clear();
            _centers.Clear();

            var tripos = new IntTriangularPos(TrianglePos);
            var triangleHeight = TriangleEdgeSize * NavigationConstants.SQRT_OF_THREE_HALVED;
            _vertices.Add(GetTriangleVerticesCommand.Execute(tripos, triangleHeight, offset: 0f));

            var center = TriangularMath.TriangularToWorld(tripos, triangleHeight).xz;
            using var centersArray = SubdivideTriangleCommand.CreateDataArray(TrianglesPerEdge, Allocator.Temp);
            var protocol = new TriangleSubdivisionProtocol()
            {
                Centers = centersArray,
                RaycastTrianglesPerEdge = TrianglesPerEdge,
                TriangleHeight = triangleHeight
            };
            SubdivideTriangleCommand.Execute(tripos, protocol);
            _innerRadius = protocol.SubdividedTriangleHeight * NavigationConstants.DIV_THREE;

            var edge = TriangleEdgeSize / TrianglesPerEdge;
            for (var i = 0; i < centersArray.Length; i++)
            {
                var data = centersArray[i];
                var vertices = GetTriangleVerticesCommand.Execute(data.WorldPos, data.IsPeak, edge);
                _vertices.Add(vertices);
                _centers.Add(data.WorldPosV3);
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
                Handles.DrawLine(tri.PinnaclePos, tri.LeftBasisPos);
                Handles.DrawLine(tri.LeftBasisPos, tri.RightBasisPos);
                Handles.DrawLine(tri.RightBasisPos, tri.PinnaclePos);
            }

            var vup = Vector3.up;
            foreach (var center in _centers)
            {
                Handles.DrawSolidDisc(center, vup, _innerRadius);
            }
        }
    }
}
