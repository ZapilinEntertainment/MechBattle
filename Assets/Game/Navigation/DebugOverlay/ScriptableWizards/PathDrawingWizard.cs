using System.Threading;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using R3;
using System;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class PathDrawingWizard : ScriptableWizard
    {
        public int3 StartPos;
        public int3 EndPos;
        public int3[] BlockedCells;

        private bool _mapSettingsPresented = false;
        private bool _isDisposed = false;
        private List<(float3 start, float3 end)> _points = new();
        private List<(Vector3 pos, float value)> _pathCosts = new();
        private GUIStyle _labelStyle = new GUIStyle();
        private MapSettingsSO _mapSettings;

        private HexPathJobCollections _hexPathJobData;
        private NavigationMap _tempMap;
        private INavigationMap _map;
        private TriangularPathJobCollections _triangularPathJobData;

        private ReactiveProperty<bool> _isAsyncOperationInProgressProperty = new();
        private CompositeDisposable _compositeDisposable = new();

        [MenuItem("ZE.Navigation/Draw Navigation Path")]
        static void OpenWizard()
        {
            DisplayWizard<PathDrawingWizard>("PathDrawingWizard", "Close", "Redraw");
        }

        void OnWizardUpdate() { }

        private async void OnWizardOtherButton()
        {
            _mapSettings = NavigationDebugDataContainer.MapSettings;
            _mapSettingsPresented = _mapSettings != null;
            errorString = _mapSettingsPresented ? string.Empty : "No map settings found";

            _points.Clear();
            _pathCosts.Clear();
            errorString = string.Empty;

            // INITIAL DATA

            var startPos = new IntTriangularPos(StartPos);
            var endPos = new IntTriangularPos(EndPos);
            var startHex = TriangularMath.TriangularToHex(startPos, _mapSettings.TriangleHeight, _mapSettings.HexEdgeSize);
            var endHex = TriangularMath.TriangularToHex(endPos, _mapSettings.TriangleHeight, _mapSettings.HexEdgeSize);

            if (math.all(startHex == endHex))
            {
                //single hex
                CalculateTrianglePath(startHex, startPos, endPos);
                DrawTrianglePath(startPos, endPos );
            }
            else
            {

                // CALCULATE HEX PATH   
                _isAsyncOperationInProgressProperty.Value = true;
                var result = await GetShortestHexPathCommand.Execute(startPos, endPos, GetMap(), GetHexPathJobData());
                _isAsyncOperationInProgressProperty.Value = false;
                if (_isDisposed)
                    return;

                if (!result.IsSuccess)
                {
                    errorString = "No path found";
                    return;
                }

                var points = result.Path;
                if (points.Count == 1)
                {
                    errorString = "path invalid";
                    return;
                }

                _isAsyncOperationInProgressProperty.Value = true;

                //todo:
                // 1) build a path from start point to edge triangle
                // 2) define edge triangles

                await CalculateTrianglePathAsync(startHex, startPos, endPos);

                for (var i = 1; i < points.Count; i++)
                {
                    var startNode = points[i-1];
                    var endNode = points[i];

                    var edgePoints = GetHexTransitionTriangles(startNode, endNode);
                    if (edgePoints.start == edgePoints.end)
                    {
                        Debug.LogError("stopped");
                        break;
                    }

                    await CalculateTrianglePathAsync(startNode.HexCoord, edgePoints.start, edgePoints.end);
                }

                _isAsyncOperationInProgressProperty.Value = false;
            }


            // BLOCKING:
            //if (BlockedCells != null) 
            //{ 
            //    var setupData = jobData.SetupData;           
            //    for (var i = 0; i < BlockedCells.Length; i++)
            //    {
            //        var pos =new IntTriangularPos(BlockedCells[i]);
            //        setupData.Set(pos, new(false, 0, 0));
            //    }            
            //}

            // CALCULATING            
           
            SceneView.RepaintAll();
        }

        private (IntTriangularPos start, IntTriangularPos end) GetHexTransitionTriangles(HexPathNodeKey start, HexPathNodeKey end)
        {
            if (!math.all(start.HexCoord == end.HexCoord))
            {
                end = end.ToOpposite();
                if (!math.all(start.HexCoord == end.HexCoord))
                {
                    Debug.Log($"some hex path error: {start.HexCoord} -> {end.HexCoord}");
                    return default;
                }
            }

            switch(start.Edge) 
            { 
                //default: 

                    // discrete logic - too much code
                    // go through edge enumerators
            }

            return default;
        }

        private HexPathJobCollections GetHexPathJobData()
        {
            _hexPathJobData ??= PrepareHexPathJobCollectionsCommand.Execute(Allocator.Persistent, _map);
            return _hexPathJobData;
        }

        private INavigationMap GetMap()
        {
            INavigationMap map = NavigationDebugDataContainer.Map;
            if (map == null)
            {
                _tempMap = new NavigationMap(_mapSettings.ToStruct());
                map = _tempMap;
            }
            return map;
        }

        private TriangularPathJobCollections GetTriangularPathJobData(int2 hexPos)
        {            
            if (_triangularPathJobData == null)
                _triangularPathJobData = PrepareTriangularPathJobCollectionsCommand.Execute(
                    Allocator.Persistent, 
                    CreateHexPos(hexPos), 
                    _mapSettings.TrianglesPerHexEdge, 
                    new FullAccessFlowMap());

            else
                _triangularPathJobData.ChangeCenter(CreateHexPos(hexPos));

            return _triangularPathJobData;
        }

        private NavigationHexPosition CreateHexPos(int2 pos) => new(pos.x, pos.y, _mapSettings.HexEdgeSize, _mapSettings.TriangleHeight);

        private void OnEditorUpdate()
        {
            Debug.Log("editor update");
        }

        // todo: move to own TrianglePathMaster
        private async Awaitable CalculateTrianglePathAsync(int2 hexPos, IntTriangularPos start, IntTriangularPos end)
        {
            var handle = LaunchTriangularPathJob(hexPos, start, end);
            while (!handle.IsCompleted)
                await Awaitable.NextFrameAsync();
            handle.Complete();
        }

        private void CalculateTrianglePath(int2 hexPos, IntTriangularPos start, IntTriangularPos end)
        {
            var handle = LaunchTriangularPathJob(hexPos, start, end);
            handle.Complete();
        }

        private JobHandle LaunchTriangularPathJob(int2 hexPos, IntTriangularPos start, IntTriangularPos end)
        {
            var jobData = GetTriangularPathJobData(hexPos);
            var job = new ConstructTriangularPathJob()
            {
                Start = start,
                End = end,
                CalculationData = jobData.CalculationData,
                SetupData = jobData.SetupData,
                OpenedList = jobData.OpenedList,
                ResultList = jobData.ResultList,
            };
            return job.ScheduleByRef();
        }

        private void DrawTrianglePath(IntTriangularPos start, IntTriangularPos end)
        {
            if (_triangularPathJobData.ResultList.Length < 2)
            {
                return;
                //Debug.Log("path is too short");
            }
            else
            {
                for (var i = 1; i < _triangularPathJobData.ResultList.Length; i++)
                {
                    var pos1 = _triangularPathJobData.ResultList[i - 1];
                    var pos2 = _triangularPathJobData.ResultList[i];
                    _points.Add(
                        (TriangularMath.TriangularToWorld(pos1, _mapSettings.TriangleHeight),
                        TriangularMath.TriangularToWorld(pos2, _mapSettings.TriangleHeight)));
                }
            }

            var coordsConverter = _triangularPathJobData.SetupData.CoordsConverter;
            for (var i = 0; i < _triangularPathJobData.CalculationData.Length; i++)
            {
                var setupData = _triangularPathJobData.SetupData[i];
                if (!setupData.IsValid)
                    continue;

                var pos = TriangularMath.TriangularToWorld(coordsConverter.IndexToTriangular(i), _mapSettings.TriangleHeight);
                if (!setupData.IsPassable)
                {
                    _pathCosts.Add((pos, -1));
                    continue;
                }

                var calculationData = _triangularPathJobData.CalculationData[i];
                _pathCosts.Add((pos, calculationData.PathCost));
            }
        }

        void OnWizardCreate() { }


        void OnEnable()
        {
            _labelStyle.richText = true;
            SceneView.duringSceneGui += OnSceneGUI;

            // generated by Google AI
            var editorUpdate = Observable.FromEvent(
                 h => EditorApplication.update += new EditorApplication.CallbackFunction(h),
                 h => EditorApplication.update -= new EditorApplication.CallbackFunction(h)
             );

            _isAsyncOperationInProgressProperty
            .DistinctUntilChanged()
            .Select(active => active
             ? editorUpdate.ThrottleFirst(TimeSpan.FromMilliseconds(100)) 
             : Observable.Empty<Unit>())
             .Switch()
            .Subscribe(_ => OnEditorUpdate())
            .AddTo(_compositeDisposable);
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            _tempMap?.Dispose();

            if (_isAsyncOperationInProgressProperty.Value)
                DisposeAsync();
            else
                FinalDispose();
        }

        private async void DisposeAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                Debug.LogWarning("job calculations still not finished, waiting for complete...");
                await _isAsyncOperationInProgressProperty
                    .Where(x => x == false)
                    .FirstAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("dispose timeout! Did you forget to set async flag to false?");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                FinalDispose();
                Debug.LogWarning("all disposables disposed");
            }
        }

        private void FinalDispose()
        {
            _isDisposed = true;
            _compositeDisposable.Dispose();
            _isAsyncOperationInProgressProperty.Dispose();
            _hexPathJobData?.Dispose();
            _triangularPathJobData?.Dispose();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (!_mapSettingsPresented)
                return;

            foreach (var pts in _points)
            {
                Handles.DrawLine(pts.start, pts.end);
            }

            foreach (var costInfo in _pathCosts)
            {
                var cost = costInfo.value;
                if (cost < 0)
                {
                    Handles.Label(costInfo.pos, "<color=red>X</color>", _labelStyle);
                }
                else
                {
                    Handles.Label(costInfo.pos, costInfo.value.ToString());
                }
            }
        }
    }
}
