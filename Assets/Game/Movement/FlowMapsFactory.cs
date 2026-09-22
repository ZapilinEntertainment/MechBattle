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
        private int _nextId = 1;
        

        public FlowMapsFactory(INavigationMap map)
        {
            _map = map;
            _rowsTable = TrianglesToIndexFlattenedConverter.FulfilRowIndices(Allocator.Persistent, _map.TrianglesPerHexEdge);
            _flattenedArrayLength = _map.HexTrianglesCount;
        }

        public void Dispose()
        {
            _rowsTable.Dispose();
        }

        public PortalExitFlowMap CreateEmptyPortalExitFlowMap(int2 hexCoord, NavigationPortalExit portalExit)
        {
            var id = _nextId++;
            var coordsConverter = CreateHexCoordsConverter(hexCoord);

            return new PortalExitFlowMap(
                id,
                hexCoord, 
                coordsConverter, 
                _flattenedArrayLength);
        }

        private FlattenedHexCoordsConverter CreateHexCoordsConverter(int2 hexCoord)
        {
            var hexPos = new NavigationHexPosition(hexCoord, _map);
            return new (
                hexPos.TriangularCenterPos,
                _map.TrianglesPerHexEdge,
                _map.HexEdgeLength,
                _map.TriangleHeight,
                _rowsTable.AsReadOnly());
        }
    
    }
}
