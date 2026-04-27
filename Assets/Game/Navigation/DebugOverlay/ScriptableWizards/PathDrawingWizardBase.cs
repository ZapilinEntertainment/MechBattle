using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public abstract class PathDrawingWizardBase<T> : ScriptableWizard where T: TriangularPathBuilderBase
    {
        public int3 StartPos = new(-2, 2, 1);
        public int3 EndPos = new(11, 14, -26);

        protected List<(float3 start, float3 end)> _points = new();
        protected NavigationMap _map;
        protected T _pathBuilder;

        void OnWizardUpdate() { }

        protected abstract void Draw();
        protected virtual void OnWizardEnabled() { }
        protected virtual void OnWizardDisabled() { }
        protected abstract INavigationMap GetMap();
        protected abstract T GetPathBuilder();

        protected void Redraw()
        {
            _points.Clear();

            if (NavigationDebugDataContainer.Map == null || NavigationDebugDataContainer.MapSettings == null)
            {
                errorString = "map and map settings required";
                return;
            }
            else
            {
                errorString = string.Empty;
            }

            _pathBuilder = GetPathBuilder();

            Draw();
        }


        void OnEnable()
        {
            GetMap();
            OnWizardEnabled();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            OnWizardDisabled();
            _map?.Dispose();
            _pathBuilder?.Dispose();
        }


        virtual protected void OnSceneGUI(SceneView sceneView)
        {
            if (_map != null)
            {
                Handles.color = Color.yellow;
                Handles.DrawSolidDisc(_map.GetWorldPos(StartPos), Vector3.up, _map.TriangleHeight / 3f);
                Handles.DrawSolidDisc(_map.GetWorldPos(EndPos), Vector3.up, _map.TriangleHeight / 3f);
            }

            foreach (var pts in _points)
            {
                Handles.DrawLine(pts.start, pts.end);
            }
        }

    }
}
