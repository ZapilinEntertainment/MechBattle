using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using VContainer;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Navigation.DebugOverlay;

namespace ZE.MechBattle.Develop
{
    public class PortalExitDebugDrawer : MonoBehaviour
    {
        private class HexDrawData
        {
            public List<TriangleDrawData> DrawData = new();
            public int LastHexVersion;
        }

        private float _nextCheckTime = 0f;
        private PortalExitsList _portalExits;
        private INavigationMap _map;
        private Dictionary<int2, HexDrawData> _hexDrawData = new();
        private List<IntTriangularPos> _trianglesList = new();
        private const float CHECK_INTERVAL = 0.5f;

        [Inject]
        public void Inject(PortalExitsList exitsList, INavigationMap map)
        {
            _portalExits = exitsList;
            _map = map;
        }

        private void Update()
        {
            if (Time.time < _nextCheckTime)
                return;
            _nextCheckTime = Time.time + CHECK_INTERVAL;

            foreach (var hex in _map.Hexes)
            {
                var hexCoord = hex.HexCoordinate;
                if (!_hexDrawData.TryGetValue(hexCoord, out var hexDrawData))
                {
                    hexDrawData = new();
                    _hexDrawData.Add(hexCoord, hexDrawData);
                }

                if (hexDrawData.LastHexVersion != hex.PassabilityVersion)
                {
                    RedrawHex(hexDrawData, hex);
                    hexDrawData.LastHexVersion = hex.PassabilityVersion;


                    Debug.Log($"redraw {hex.HexCoordinate} v.{hex.PassabilityVersion} : {hex.PortalExitIds.Count}");
                }
            }
        }

        private void RedrawHex(HexDrawData drawData, INavigationHex hex)
        {
            drawData.DrawData.Clear();
            foreach (var exitId in hex.PortalExitIds)
            {
                if (!_portalExits.TryGetValue(exitId, out var exitData))
                {
                    Debug.LogWarning($"exit {exitId} of {hex.HexCoordinate} doesnt exists");
                    continue;
                }

                if (exitData.Length == 1)
                {
                    drawData.DrawData.Add(TrianglesDrawHelper.GetDrawData(exitData.StartTriangle, _map));
                }
                else
                {
                    var peakDir = exitData.Edge.ToAlongsidePeakDirection();
                    var valleyDir = exitData.Edge.ToAlongsideValleyDirection();

                    var tripos = exitData.StartTriangle;
                    drawData.DrawData.Add(TrianglesDrawHelper.GetDrawData(tripos, _map));

                    for (var i = 1; i < exitData.Length; i++)
                    {
                        if (tripos.IsPeak)
                            tripos = TriangularMath.GetPeakNeighbour(tripos, peakDir);
                        else
                            tripos = TriangularMath.GetValleyNeighbour(tripos, valleyDir);

                        drawData.DrawData.Add(TrianglesDrawHelper.GetDrawData(tripos, _map));
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            var previousZTest = TrianglesDrawHelper.SwitchZTestAndSave();
            foreach (var hexDrawData in _hexDrawData.Values)
            {
                foreach (var trisDrawData in hexDrawData.DrawData)
                {
                    TrianglesDrawHelper.DrawHandles(trisDrawData);
                }                
            }
            TrianglesDrawHelper.RestoreZTest(previousZTest);
        }
    }
}
