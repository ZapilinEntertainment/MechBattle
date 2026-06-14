using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace ZE.MechBattle
{
    public class PortalConnectionsList : IEnumerable<KeyValuePair<int, Dictionary<int,float>>>
    {
        public int Version { get; private set; }
        private Dictionary<int, Dictionary<int, float>> _connections = new();

        public void AddConnection(int portalIdA, int portalIdB, float distance) 
        {
            i_AddConnection(portalIdA, portalIdB,distance);
            i_AddConnection(portalIdB, portalIdA,distance);
            Version++;
        }

        public void RemoveConnection(int portalIdA, int portalIdB)
        {
            if (_connections.TryGetValue(portalIdA, out var connectionsA))
                connectionsA.Remove(portalIdB);

            if (_connections.TryGetValue(portalIdB, out var connectionsB))
                connectionsB.Remove(portalIdA);
        }

        public void RemoveConnection(int portalId)
        {
            if (_connections.TryGetValue(portalId, out var portalConnections))
            {
                var connectionsCount = portalConnections.Count;
                if (connectionsCount == 0)
                    return;

                foreach (var connectedPortalData in portalConnections)
                {
                    var connectedPortalId = connectedPortalData.Key;
                    if (_connections.TryGetValue(connectedPortalId, out var connectedPortalOwnConnections))
                        connectedPortalOwnConnections.Remove(portalId);
                }

                _connections.Remove(portalId);
            }
        }

        public float GetDistance(int portalIdA, int portalIdB) => _connections[portalIdA][portalIdB];

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

        private void i_AddConnection(int portalIdA, int portalIdB, float distance)
        {
            if (!_connections.TryGetValue(portalIdA, out var connectionsList) || connectionsList == null)
            {
                connectionsList = new();
                _connections[portalIdA] = connectionsList;
            }

            connectionsList[portalIdB] = distance;
        }

        public IEnumerator<KeyValuePair<int, Dictionary<int, float>>> GetEnumerator() => _connections.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
