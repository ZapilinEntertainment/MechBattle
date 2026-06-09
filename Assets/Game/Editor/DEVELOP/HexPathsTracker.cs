using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using TriInspector;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Develop
{
    public class HexPathsTracker : MonoBehaviour
    {
        [Serializable]
        public struct SerializedHexPathData
        {
            [PropertyOrder(0)]public int PathId;
            public bool IsCalculated;
            public bool ReachesTarget;
            public HexEdgeKey[] Points;

            [HideInInspector] public (HexEdgeKey, HexEdgeKey) Destinations;
            [ShowInInspector, PropertyOrder(1)]private string DestinationsString => $"{Destinations.Item1} -> {Destinations.Item2}";
        }


        [ShowInInspector, PropertyOrder(0)] private int Version => _lastDrawnVersion;
        [ReadOnly, SerializeField] private List<SerializedHexPathData> _data;
        private HexPortalsList _hexPaths;
        private int _lastDrawnVersion = -1;


        [Inject]
        public void Inject(HexPortalsList hexPaths)
        {
            _hexPaths = hexPaths;
        }

        private void Update()
        {
            if (_hexPaths == null)
            {
                enabled = false;
                return;
            }

            //if (_lastDrawnVersion != _hexPaths.PathDataVersion)
            //{
            //    _lastDrawnVersion =  _hexPaths.PathDataVersion;
            //    _data.Clear();
            //    foreach (var hexPath in _hexPaths)
            //    {
            //        _data.Add(new()
            //        {
            //            IsCalculated = hexPath.IsCalculated,
            //            PathId = hexPath.Id,
            //            Destinations = hexPath.DestinationKey,
            //            Points = hexPath.Points,
            //            ReachesTarget = hexPath.HasReachedTarget
            //        });
            //    }
            //}
        }
    }
}
