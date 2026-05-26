using System.Collections.Generic;

namespace ZE.MechBattle
{
    public class PortalConnectionsList
    {
        public int Version { get; private set; }
        private Dictionary<int, Dictionary<int, float>> _connections = new();

        public void AddConnection(int portalIdA, int portalIdB, float distance) 
        {
            if (!_connections.TryGetValue(portalIdA, out var connectionsList) || connectionsList == null)
            {
                connectionsList = new();
                _connections[portalIdA] = connectionsList;
            }

            connectionsList[portalIdB] = distance;
            Version++;
        }

        public bool TryGetPortalConnections(int portalId, out IReadOnlyDictionary<int, float> connections)
        {
            if (_connections.TryGetValue(portalId, out var rawConnections) && (rawConnections != null) && (rawConnections.Count != 0))
            {
                connections = rawConnections;
                return true;
            }

            connections = null; 
            return false;
        }
            

    }
}
