using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Navigation.DebugOverlay;

public class HexSectorEnumeratorDrawWizard : ScriptableWizard
{
    [SerializeField] private int _radiusInUnits = 25;
    [SerializeField] private float _triangleEdgeSize = 10f;
    [Space]
    [SerializeField] private int2 _hexCoord;
    [SerializeField] private HexSector _sector = HexSector.Top;
    [SerializeField] private int _rowIndex = 0;
    private List<TriangleVertices> _drawData = new();

    [MenuItem("ZE.Navigation/Enumerate Hex Sectors")]
    static void OpenWizard()
    {
        DisplayWizard<HexSectorEnumeratorDrawWizard>("Hex Sector Enumerator Draw Wizard", "Close", "Draw");
    }

    void OnWizardCreate() { }

    private void OnWizardOtherButton()
    {
        _drawData.Clear();

        var triangleHeight = TriangularMath.GetTriangleHeight(_triangleEdgeSize);
        var innerRingTriangles = NavigationMapHelper.GetSixInnerRingTriangles();
        var startTripos = innerRingTriangles[(int)_sector];

        var setupData = new HexSectorRowsEnumerationSetup(_sector, startTripos);
        foreach (var tripos in new HexSectorRowEnumerator(setupData, _rowIndex))
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
    }
}
