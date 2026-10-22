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
        private CellResultsListOperator _resultsOperator = new(32);

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
            AffectedTrianglesCalculator.CalculateRoundAffectedTrianglesManaged(_resultsOperator, _centerPosInUnits, _radiusInUnits, triangleHeight);

            foreach (var tripos in _resultsOperator.List)
            {
                _drawData.Add(GetTriangleVerticesCommand.Execute(tripos, triangleHeight, offset: 0f));
            }
            _resultsOperator.Clear();

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


