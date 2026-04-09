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
            public readonly bool IsPassable;

            public RaycastPointData(Vector3 pos, Color color, bool isPassable)
            {
                Pos = pos;
                Color = color;
                IsPassable = isPassable;
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

            var hexCenterWorld = HexMath.HexToWorld(hexCoord, caster.HexEdgeSize);
            var job = caster.ConstructPositionsJob(hexCenterWorld, NavigationConstants.GetGroundCastQueryParameters());
            var handle = job.ScheduleByRef();
            handle.Complete();

            _points.Clear();

            var color = Color.white;
            foreach (var command in job.RaycastCommands)
            {                
                var point = command.from;
                point.y = 0;
                _points.Add(new(point, color, true));
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

            var hexCenterWorld = HexMath.HexToWorld(hexCoord, caster.HexEdgeSize);
            using var raycastData = await caster.CastHexAsync(Unity.Collections.Allocator.Temp, hexCenterWorld, NavigationConstants.GetGroundCastQueryParameters(), cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            _points.Clear();
            foreach (var hit in raycastData) 
            {
                Color color;
                var isPassable = false;
                var pos = hit.point;

                if (hit.collider == null)
                {
                    // actually, this will never happen, need to rework cast hex async output
                    color = _noColliderRayColor;
                    pos = new Vector3(pos.x, 0.01f, pos.y);
                }                    
                else
                {
                    var t = (hit.distance / COLOR_HEIGHT_STEP) - Mathf.Floor(hit.distance / COLOR_HEIGHT_STEP);
                    color = Color.Lerp(_shortestRayColor, _longestRayColor, t);
                    pos.y += 0.01f;

                    isPassable = !hit.collider.CompareTag(NavigationConstants.OBSTACLE_TAG);
                }
                _points.Add(new(pos, color, isPassable));
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
                if (point.IsPassable)
                    Handles.DrawSolidDisc(point.Pos, up, 1f);
                else
                    Handles.DrawWireDisc(point.Pos, up, 1f);
            }
            Handles.zTest = previousZTest;
        }

    }
}
