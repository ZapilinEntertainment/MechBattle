using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class NavigationCastDrawer
    {
        private readonly struct RaycastPointData
        {
            public readonly Vector3 Pos;
            public readonly Color Color;

            public RaycastPointData(Vector3 pos, Color color)
            {
                Pos = pos;
                Color = color;
            }
        }

        private readonly List<RaycastPointData> _points = new();
        private readonly Color _shortestRayColor = Color.white;
        private readonly Color _longestRayColor = Color.yellow;
        private readonly Color _noColliderRayColor = Color.red;
        private const float COLOR_HEIGHT_STEP = 50f;

        public void ShowCastPoints(int2 hexCoord)
        {
            var caster = NavigationDebugDataContainer.Caster as NavigationCaster; 
            if (caster == null)
            {
                Debug.LogError("no caster found or caster is not NavigationCaster");
                return;
            }

            var job = caster.ConstructPositionsJob(hexCoord, NavigationConstants.GetGroundCastQueryParameters());
            var handle = job.ScheduleByRef();
            handle.Complete();

            _points.Clear();

            var color = Color.white;
            foreach (var command in job.RaycastCommands)
            {                
                _points.Add(new(command.from, color));
            }            
        }

        public async Awaitable CastHexAsync(int2 hexCoord, CancellationToken cancellationToken)
        {
            var caster = NavigationDebugDataContainer.Caster;
            if (caster == null)
            {
                Debug.LogError("no caster found");
                return;
            }

            using var raycastData = await caster.CastHexAsync(hexCoord, NavigationConstants.GetGroundCastQueryParameters(), cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            _points.Clear();
            foreach (var point in raycastData) 
            {
                Color color;
                if (point.collider == null)
                {
                    color = _noColliderRayColor;
                }                    
                else
                {
                    var t = (point.distance / COLOR_HEIGHT_STEP) - Mathf.Floor(point.distance / COLOR_HEIGHT_STEP);
                    color = Color.Lerp(_shortestRayColor, _longestRayColor, t);
                }
                _points.Add(new(point.point + 0.01f * Vector3.up, color));
            }
        }

        public void Clear()
        {
            var count = _points.Count;
            _points.Clear();
            Debug.Log($"{count} points cleared");
        }

        public void OnSceneGUI()
        {
            if (_points.Count == 0)
                return;

            var previousZTest = Handles.zTest;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            var up = Vector3.up;
            foreach (var point in _points)
            {
                Handles.color = point.Color;
                Handles.DrawSolidDisc(point.Pos, up, 1f);
            }
            Handles.zTest = previousZTest;
        }

    }
}
