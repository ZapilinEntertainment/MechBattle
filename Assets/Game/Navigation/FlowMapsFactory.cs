using System;
using Unity.Collections;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class FlowMapsFactory : IDisposable
    {
        private readonly NativeArray<byte> _rowsTable;
        private readonly INavigationMap _map;
        private readonly int _flattenedArrayLength;
        

        public FlowMapsFactory(INavigationMap map)
        {
            _map = map;
            _rowsTable = TrianglesToIndexFlattenedConverter.FulfilRowIndices(Allocator.Persistent, _map.TrianglesPerHexEdge);
            _flattenedArrayLength = _map.TrianglesPerHex;
        }

        public void Dispose()
        {
            _rowsTable.Dispose();
        }

        public FlowMap CreateEmptyFlowMap(int2 hexCoord)
        {
            var hexPos = new NavigationHexPosition(hexCoord, _map);
            return new FlowMap(hexCoord, new( hexPos.TriangularCenterPos, _map.TrianglesPerHexEdge, _map.HexEdgeLength, _map.TriangleHeight, _rowsTable.AsReadOnly()), _flattenedArrayLength);
        }
    
    }
}
