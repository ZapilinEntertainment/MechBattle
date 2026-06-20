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
        [Serializable]
        private struct SerializableExitData
        {
            public int Id;
            public int3 Start;
            public int Length;
            public int ZoneIndex;
            public HexEdge Edge;
            public int3 Center;

            public SerializableExitData(int id, NavigationPortalExit exit)
            {
                Id = id;
                Start = exit.StartTriangle;
                Length = exit.Length;
                ZoneIndex = exit.ZoneIndex;
                Center = exit.Center;
                Edge = exit.Edge;
            }
        }

        [SerializeField] private CompareFunction _compareFunction = CompareFunction.LessEqual;
        [SerializeField, ReadOnly] private int _currentExitListVersion = 0;
        [SerializeField, ReadOnly] private int _exitsCount = 0;
        [SerializeField, ReadOnly] private List<SerializableExitData> _exitsList = new();
        private IPortalExitsList _exits;     
        private INavigationMap _map;
        private List<(TriangleDrawData, int)> _drawData = new();
        private List<IntTriangularPos> _trianglesList = new();        
        private static readonly Color[] _colors = new Color[]
        {
            Color.green,
            Color.softYellow,
            Color.brown,
            Color.darkOrange,
            Color.blue,
            Color.deepPink,
            Color.lightBlue,
            Color.lavender,
            Color.violet,
            Color.cyan,
        };

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

            _exitsList.Clear();
            foreach (var exitKvp in _exits)
            {
                _exitsList.Add(new(exitKvp.Key, exitKvp.Value));
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
                    _drawData.Add((TrianglesDrawHelper.GetDrawData(exitData.StartTriangle, _map), exitId));
                    _exitsCount++;
                }
                else
                {
                    var peakDir = exitData.Edge.ToAlongsidePeakDirection();
                    var valleyDir = exitData.Edge.ToAlongsideValleyDirection();

                    var tripos = exitData.StartTriangle;
                    _drawData.Add((TrianglesDrawHelper.GetDrawData(tripos, _map), exitId));

                    for (var i = 1; i < exitData.Length; i++)
                    {
                        if (tripos.IsPeak)
                            tripos = TriangularMath.GetPeakNeighbour(tripos, peakDir);
                        else
                            tripos = TriangularMath.GetValleyNeighbour(tripos, valleyDir);

                        _drawData.Add((TrianglesDrawHelper.GetDrawData(tripos, _map), exitId));
                    }
                    _exitsCount += exitData.Length;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            var previousZTest = TrianglesDrawHelper.SwitchZTestAndSave(_compareFunction);
            foreach (var drawDataPack in _drawData)
            {
                var index = drawDataPack.Item2;
                Handles.color = _colors[index % _colors.Length];
                TrianglesDrawHelper.DrawHandles(drawDataPack.Item1);
            }
            TrianglesDrawHelper.RestoreZTest(previousZTest);
        }
    }
}
