using System;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using UnityEngine.Rendering;
using System.Collections.Generic;
using VContainer;
using TriInspector;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Navigation.DebugOverlay;

namespace ZE.MechBattle.Develop
{
    public class PortalExitDebugDrawer : MonoBehaviour
    {
        [SerializeField] private int _portalId;
        [SerializeField] private CompareFunction _drawCompareFunction = CompareFunction.LessEqual;
        private IHexPortalsList _portals;
        private IPortalExitsList _exits;     
        private INavigationMap _map;
        private List<TriangleDrawData> _drawDataA = new();
        private List<TriangleDrawData> _drawDataB = new();
        private readonly Color _colorA = Color.green;
        private readonly Color _colorB = Color.blue;

        [Inject]
        public void Inject(IPortalExitsList exitsList, INavigationMap map, IHexPortalsList portals)
        {
            _exits = exitsList;
            _map = map;
            _portals = portals;
        }

        [Button("Draw portal exits")]
        private void DrawPortalExits()
        {
            _drawDataA.Clear();
            _drawDataB.Clear();

            if (!_portals.TryGetValue(_portalId, out var portal))
            {
                _portalId = -1;
                return;
            }

            if (_exits.TryGetValue(portal.ExitIdA, out var exitA))
            {
                foreach (var tripos in exitA.Edge.GetEdgeEnumerable(exitA))
                {
                    _drawDataA.Add(TrianglesDrawHelper.GetDrawData(tripos, _map));
                }
            }

            if (_exits.TryGetValue(portal.ExitIdB, out var exitB))
            {
                foreach (var tripos in exitB.Edge.GetEdgeEnumerable(exitB))
                {
                    _drawDataB.Add(TrianglesDrawHelper.GetDrawData(tripos, _map));
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            var previousZTest = TrianglesDrawHelper.SwitchZTestAndSave(_drawCompareFunction);

            Handles.color = _colorA;
            foreach (var drawData in _drawDataA)
            {
                TrianglesDrawHelper.DrawHandles(drawData);
            }

            Handles.color = _colorB;
            foreach (var drawData in _drawDataB)
            {
                TrianglesDrawHelper.DrawHandles(drawData);
            }

            TrianglesDrawHelper.RestoreZTest(previousZTest);
        }
    }
}
