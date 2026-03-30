using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class PathDrawingWizard : ScriptableWizard
    {
        public int3 StartPos;
        public int3 EndPos;
        public int3[] BlockedCells;

        private bool _mapSettingsPresented = false;
        private List<(float3 start, float3 end)> _points = new();

        [MenuItem("ZE.Navigation/Draw Navigation Path")]
        static void OpenWizard()
        {
            DisplayWizard<PathDrawingWizard>("PathDrawingWizard", "Close", "Redraw");
        }

        void OnWizardUpdate() { }

        private void OnWizardOtherButton()
        {
            var mapSettings = NavigationDebugDataContainer.MapSettings;
            _mapSettingsPresented = mapSettings != null;
            errorString = _mapSettingsPresented ? string.Empty : "No map settings found";

            _points.Clear();

            // INITIAL DATA

            var startPos = new IntTriangularPos(StartPos);
            var endPos = new IntTriangularPos(EndPos);
            var startHex = TriangularMath.TriangularToHex(startPos, mapSettings.TriangleHeight, mapSettings.HexEdgeSize);
            var endHex = TriangularMath.TriangularToHex(endPos, mapSettings.TriangleHeight, mapSettings.HexEdgeSize);

            if (!math.all(startHex == endHex))
            {
                errorString = "different hexes: not implemented yet";
                SceneView.RepaintAll();
                return;
            }
            else
            {
                errorString = string.Empty;
            }

            var allocator = Allocator.Persistent;
            var hexPos = new NavigationHexPosition(startHex.x, startHex.y, mapSettings.HexEdgeSize, mapSettings.TriangleHeight);
            using var jobData = PrepareTriangularPathJobCollectionsCommand.Execute(allocator, hexPos, mapSettings.TrianglesPerHexEdge, new FullAccessFlowMap());


            // BLOCKING:
            if (BlockedCells != null) 
            { 
                var setupData = jobData.SetupData;           
                for (var i = 0; i < BlockedCells.Length; i++)
                {
                    var pos =new IntTriangularPos(BlockedCells[i]);
                    setupData.Set(pos, new(false, 0, 0));
                }            
            }

            // CALCULATING
            var job = new ConstructTriangularPathJob()
            {
                Start = startPos,
                End = endPos,
                CalculationData = jobData.CalculationData,
                SetupData = jobData.SetupData,
                OpenedList = jobData.OpenedList,
                ResultList = jobData.ResultList,
            };
            var handle = job.ScheduleByRef();
            handle.Complete();

            if (job.ResultList.Length < 2)
            {
                SceneView.RepaintAll();
                return;
            }
            else
            {
                Debug.Log("path is too short");
            }

            for (var i = 1; i < job.ResultList.Length; i++)
            {
                var pos1 = job.ResultList[i - 1];
                var pos2 = job.ResultList[i];
                _points.Add(
                    (TriangularMath.TriangularToWorld(pos1, mapSettings.TriangleHeight),
                    TriangularMath.TriangularToWorld(pos2, mapSettings.TriangleHeight)));
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
