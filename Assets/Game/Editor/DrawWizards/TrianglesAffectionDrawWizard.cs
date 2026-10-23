using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Navigation.DebugOverlay;

namespace ZE.MechBattle.DrawWizards
{
    public class TrianglesAffectionDrawWizard : ScriptableWizard
    {
        [SerializeField] private Vector3 _centerPosInUnits;
        [SerializeField] private float3 _rotationEuler;
        [Space]
        [SerializeField] private BlockZoneMode _mode;
        [SerializeField] private int _value1;
        [SerializeField] private int _value2;
        [Space]
        [SerializeField] private float _triangleEdgeSize = 10f;
        private List<TriangleVertices> _drawData = new();
        private CellResultsListOperator _resultsOperator = new(32);

        private RigidTransform CenterPoint => new (quaternion.Euler(math.radians(_rotationEuler)), _centerPosInUnits);
        private float2 Size2D => new(_value1, _value2);

        [MenuItem("ZE.Navigation/Triangulate zone")]
        static void OpenWizard()
        {
            DisplayWizard<TrianglesAffectionDrawWizard>("Triangulate Draw Wizard", "Close", "Draw");
        }

        void OnWizardUpdate()
        {
            
        }

        void OnWizardCreate() { }

        private void OnWizardOtherButton()
        {
            _drawData.Clear();

            var triangleHeight = TriangularMath.GetTriangleHeight(_triangleEdgeSize);
            switch (_mode)
            {
                case BlockZoneMode.Round:
                    {
                        AffectedTrianglesCalculator.CalculateRoundAffectedTrianglesManaged(_resultsOperator, _centerPosInUnits, _value1, triangleHeight);
                        break;
                    }
                case BlockZoneMode.Rect:
                    {
                        AffectedTrianglesCalculator.CalculateRectAffectedTrianglesManaged(
                            _resultsOperator, 
                            CenterPoint, 
                            Size2D, 
                            triangleHeight);
                        break;
                    }
                default:
                    {
                        throw new System.NotImplementedException($"{_mode} not implemented");
                    }
            }
           

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

            ZoneDrawer.Draw(CenterPoint, Size2D, _mode);            
        }

       
    }
}


