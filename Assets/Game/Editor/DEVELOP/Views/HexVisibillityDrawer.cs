using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using System.Collections.Generic;
using TriInspector;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Views;

namespace ZE.MechBattle.Develop
{
    public class HexVisibilityDrawer : MonoBehaviour
    {
        [SerializeField] private KeyCode _redrawKey = KeyCode.Space;
        private HexPointsPreset _preset;
        private CameraController _cameraController;
        private INavigationMap _map;
        private List<int3> _visibilityMasks = new();

        private bool _isInjected = false;
        private Filter _hexesFilter;
        private Stash<HexComponent> _hexComponents;


        [Inject]
        public void Inject(INavigationMap navigationMap, CameraController cameraController, World world)
        {
            _map = navigationMap;            
            _cameraController = cameraController;

            _preset = new(_map.HexEdgeLength);

            _hexesFilter = world.Filter.With<HexComponent>().Build();
            _hexComponents = world.GetStash<HexComponent>();

            _isInjected = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_redrawKey))
            {
                if (!_isInjected)
                {
                    UnityEngine.Debug.LogError("utility not injected");
                    return;
                }
                RecalculateVisibility();
            }
                
        }

        private void RecalculateVisibility()
        {
            var nativeFilter = _hexesFilter.AsNative();
            var nativeHexStash = _hexComponents.AsNative();
            using var sectorVisibilityMasks = new NativeArray<int>(nativeFilter.length, Allocator.TempJob);

            var vertices = new NativeArray<float2>(6, Allocator.TempJob);
            vertices[0] = _preset.TopLeft;
            vertices[1] = _preset.TopRight;
            vertices[2] = _preset.Right;
            vertices[3] = _preset.BottomRight;
            vertices[4] = _preset.BottomLeft;
            vertices[5] = _preset.Left;
            

            var job = new HexVisibilityCheckJob()
            {
                CameraData = new(_cameraController.Camera, _map.HexEdgeLength * 1.5f),
                HexesFilter = nativeFilter,
                HexVertices = vertices,
                HexComponents = nativeHexStash,
                HexEdge = _map.HexEdgeLength,
                VisibilityMasks = sectorVisibilityMasks
            };
            job.Run();

            _visibilityMasks.Clear();
            for (var i = 0; i < nativeFilter.length; i++)
            {
                var hexEntity = nativeFilter[i];
                var hexCoord = _hexComponents.Get(hexEntity).Coord;
                _visibilityMasks.Add(new int3(hexCoord, sectorVisibilityMasks[i]));
            }

            vertices.Dispose();
        }


#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (!_isInjected || _visibilityMasks.Count == 0)
                return;

            foreach (var val in _visibilityMasks)
            {
                var hexCoord = val.xy;
                var sectorsVisibilityMask = val.z;

                var center = HexMath.HexToWorld(hexCoord, _map.HexEdgeLength);
                
                if ((sectorsVisibilityMask & (1 << (int)HexSector.Top)) != 0)
                    DrawSector(center, center + _preset.TopLeft, center + _preset.TopRight);

                if ((sectorsVisibilityMask & (1 << (int)HexSector.TopRight)) != 0)
                    DrawSector(center, center + _preset.TopRight, center + _preset.Right);

                if ((sectorsVisibilityMask & (1 << (int)HexSector.BottomRight)) != 0)
                    DrawSector(center, center + _preset.Right, center + _preset.BottomRight);

                if ((sectorsVisibilityMask & (1 << (int)HexSector.Bottom)) != 0)
                    DrawSector(center, center + _preset.BottomRight, center + _preset.BottomLeft);

                if ((sectorsVisibilityMask & (1 << (int)HexSector.BottomLeft)) != 0)
                    DrawSector(center, center + _preset.BottomLeft, center + _preset.Left);

                if ((sectorsVisibilityMask & (1 << (int)HexSector.TopLeft)) != 0)
                    DrawSector(center, center + _preset.Left, center + _preset.TopLeft);
            }
        }

        private void DrawSector(float2 posA, float2 posB, float2 posC)
        {
            Handles.DrawAAConvexPolygon(new Vector3(posA.x, 0f, posA.y), new Vector3(posB.x, 0f, posB.y), new Vector3(posC.x, 0f, posC.y));
        }
#endif
    }
}
