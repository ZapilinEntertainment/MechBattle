using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Navigation.DebugOverlay;
using Unity.Jobs;

namespace ZE.MechBattle.DrawWizards
{
    public class TriangleRadiusDrawWizard : ScriptableWizard
    {
        [SerializeField] private Vector3 _centerPosInUnits;
        [SerializeField] private int _radiusInUnits = 25;
        [SerializeField] private float _triangleEdgeSize = 10f;
        private List<TriangleVertices> _drawData = new();

        [MenuItem("ZE.Navigation/Draw Triangle Radius")]
        static void OpenWizard()
        {
            DisplayWizard<TriangleRadiusDrawWizard>("Triangle Radius Draw Wizard", "Close", "Draw");
        }

        void OnWizardUpdate()
        {
            
        }

        void OnWizardCreate() { }

        private void OnWizardOtherButton()
        {
            _drawData.Clear();

            var triangleHeight = TriangularMath.GetTriangleHeight(_triangleEdgeSize);
            using var resultsList = new NativeList<IntTriangularPos>(Allocator.TempJob);
            var job = new GetTrianglesInRadiusJob()
            {
                RadiusInUnits = _radiusInUnits,
                ResultList = resultsList,
                TriangleHeight = triangleHeight,
                WorldPos = _centerPosInUnits
            };
            job.Run();

            foreach (var tripos in job.ResultList)
            {
                _drawData.Add(GetTriangleVerticesCommand.Execute(tripos, triangleHeight, offset: 0f));
            }

            SceneView.RepaintAll();
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
            Handles.color = Color.white;
            foreach (var drawData in _drawData)
            {
                TrianglesDrawHelper.DrawHandles(drawData, opaque: false);
            }

            Handles.DrawSolidDisc(_centerPosInUnits, Vector3.up, 1f);
            Handles.DrawWireDisc(_centerPosInUnits, Vector3.up, _radiusInUnits);
        }
    }
}


