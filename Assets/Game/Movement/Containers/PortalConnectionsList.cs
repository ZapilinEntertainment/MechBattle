using System.Collections.Generic;

namespace ZE.MechBattle
{
    public class PortalConnectionsList
    {
        private readonly struct ConnectedPortalData
        {
            public readonly int PortalId;
            public readonly int ZoneIndex;

            public ConnectedPortalData(int portalId, int zoneIndex)
            {
                PortalId = portalId;
                ZoneIndex = zoneIndex;
            }
        }

        public int Version { get; private set; }
        private Dictionary<int, ConnectedPortalData> _connections = new();

        public void AddConnection(int portalIdA, int portalIdB, int portalBZoneIndex) 
        {
            _connections.Add(portalIdA, new(portalIdB, portalBZoneIndex));
            Version++;
        }

    }
}
