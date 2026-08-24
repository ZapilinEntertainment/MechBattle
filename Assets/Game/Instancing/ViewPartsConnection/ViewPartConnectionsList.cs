using System.Collections.Generic;

namespace ZE.MechBattle
{
    public class ViewPartConnectionsList
    {
        private readonly Dictionary<IViewConnectionsPoint, List<IConnectableViewPart>> _connectionsList = new();


        public void OnConnected(IViewConnectionsPoint point, IConnectableViewPart part)
        {
            if (!_connectionsList.TryGetValue(point, out var pointConnectionsList))
            {
                pointConnectionsList = new();
                _connectionsList.Add(point, pointConnectionsList);
            }
            pointConnectionsList.Add(part);
        }

        public void DisconnectAll(IViewConnectionsPoint point)
        {
            if (!_connectionsList.TryGetValue(point, out var pointConnectionsList))
                return;

            foreach (var connectedPart in pointConnectionsList)
            {
                connectedPart.OnDisconnected();
            }
            _connectionsList.Remove(point);
        }
    
    }
}
