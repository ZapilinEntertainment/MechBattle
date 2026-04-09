using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.DataStoring
{

    public readonly struct StoredHexData
    {        
        public byte[] Data => _data;
        public readonly FlowMapType MapType;
        public readonly HexEdgesAccessMap EdgesAccessMap;
        public readonly bool DefaultPassability;

        private readonly byte[] _data;
        
        public StoredHexData(byte[] data, FlowMapType mapType, HexEdgesAccessMap accessMap, bool defaultPassability = true)
        {
            _data = data;
            MapType = mapType;
            EdgesAccessMap = accessMap;
            DefaultPassability = defaultPassability;
        }
    }
}
