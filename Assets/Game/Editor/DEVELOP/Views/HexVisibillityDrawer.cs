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
        private HexVerticesPreset _preset;
        private INavigationMap _map;
        private List<int3> _visibilityMasks = new();

        private bool _isInjected = false;
        private Filter _hexesFilter;
        private Stash<HexComponent> _hexComponents;
        private Stash<HexSectorsVisibilityMaskComponent> _hexSectorsVisibility;


        [Inject]
        public void Inject(INavigationMap navigationMap, World world)
        {
            _map = navigationMap;            

            _preset = new(_map.HexEdgeLength);

            _hexesFilter = world.Filter.With<HexComponent>().With<HexSectorsVisibilityMaskComponent>().Build();
            _hexComponents = world.GetStash<HexComponent>();
            _hexSectorsVisibility = world.GetStash<HexSectorsVisibilityMaskComponent>();

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
            _visibilityMasks.Clear();
            foreach (var hexEntity in _hexesFilter)
            {
                var hexCoord = _hexComponents.Get(hexEntity).Coord;
                var visibilityMask = _hexSectorsVisibility.Get(hexEntity).Value;
                _visibilityMasks.Add(new int3(hexCoord, visibilityMask));
            }
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
