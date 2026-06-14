using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using VContainer;
using TriInspector;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Navigation.DebugOverlay;

namespace ZE.MechBattle.Develop
{
    public class PortalExitDebugDrawer : MonoBehaviour
    {
        [SerializeField, ReadOnly] private int _currentExitListVersion = 0;
        [SerializeField, ReadOnly] private int _exitsCount = 0;
        private IPortalExitsList _exits;
        private INavigationMap _map;
        private List<TriangleDrawData> _drawData = new();
        private List<IntTriangularPos> _trianglesList = new();

        [Inject]
        public void Inject(IPortalExitsList exitsList, INavigationMap map)
        {
            _exits = exitsList;
            _map = map;
        }

        private void Update()
        {
            if (_exits.Version == _currentExitListVersion)
                return;

            _drawData.Clear();
            _exitsCount = 0;
            foreach (var hex in _map.Hexes)
            {
                RedrawHex(hex);
            }

            _currentExitListVersion = _exits.Version;
        }

        private void RedrawHex(INavigationHex hex)
        {
            foreach (var exitId in hex.PortalExitIds)
            {
                if (!_exits.TryGetValue(exitId, out var exitData))
                {
                    Debug.LogWarning($"exit {exitId} of {hex.HexCoordinate} doesn't exists");
                    continue;
                }

                if (exitData.Length == 1)
                {
                    _drawData.Add(TrianglesDrawHelper.GetDrawData(exitData.StartTriangle, _map));
                    _exitsCount++;
                }
                else
                {
                    var peakDir = exitData.Edge.ToAlongsidePeakDirection();
                    var valleyDir = exitData.Edge.ToAlongsideValleyDirection();

                    var tripos = exitData.StartTriangle;
                    _drawData.Add(TrianglesDrawHelper.GetDrawData(tripos, _map));

                    for (var i = 1; i < exitData.Length; i++)
                    {
                        if (tripos.IsPeak)
                            tripos = TriangularMath.GetPeakNeighbour(tripos, peakDir);
                        else
                            tripos = TriangularMath.GetValleyNeighbour(tripos, valleyDir);

                        _drawData.Add(TrianglesDrawHelper.GetDrawData(tripos, _map));
                    }
                    _exitsCount += exitData.Length;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            var previousZTest = TrianglesDrawHelper.SwitchZTestAndSave();
            foreach (var drawData in _drawData)
            {
                TrianglesDrawHelper.DrawHandles(drawData);
            }
            TrianglesDrawHelper.RestoreZTest(previousZTest);
        }
    }
}
