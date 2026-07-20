using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public readonly struct HexUpdateRequest
    {
        public readonly int2 HexCoord;
        public readonly int HexPassabilityVersion;

        public HexUpdateRequest(int2 hexCoord, int hexVersion)
        {
            HexCoord = hexCoord;
            HexPassabilityVersion = hexVersion;
        }
    }

    public abstract class HexUpdateRequestsList
    {
        public int AwaitingCount => _awaitingRequests.Count;
        public int CalculatingCount => _calculatingRequests.Count;
        private readonly Dictionary<int2, int> _awaitingRequests = new();
        private readonly Dictionary<int2, int> _calculatingRequests = new();

        public void AddRequest(int2 hexCoord, int hexVersion)
        {
            if (!_awaitingRequests.TryGetValue(hexCoord, out var currentRequestedVersion) || hexVersion > currentRequestedVersion)
            {
                _awaitingRequests[hexCoord] = hexVersion;
            }
        }

        public void CancelRequest(int2 hexCoord, int version)
        {
            if (_awaitingRequests.TryGetValue(hexCoord, out var currentRequestedVersion) && version == currentRequestedVersion)
                _awaitingRequests.Remove(hexCoord);
        }

        public void OnRequestStartCalculating(int2 hexCoord, int hexVersion)
        {
            if (_awaitingRequests.TryGetValue(hexCoord, out var requestedHexVersion) && requestedHexVersion <= hexVersion)
                _awaitingRequests.Remove(hexCoord);

            if (!_calculatingRequests.TryGetValue(hexCoord, out var currentCalculatingVersion) || currentCalculatingVersion < hexVersion)
                _calculatingRequests[hexCoord] = hexVersion;
        }

        public void OnRequestCalculated(int2 hexCoord, int hexVersion)
        {
            if (_calculatingRequests.TryGetValue(hexCoord, out var currentCalculatingVersion) && currentCalculatingVersion <= hexVersion)
                _calculatingRequests.Remove(hexCoord);
        }

        public void OnRequestCalculationStopped(int2 hexCoord, int hexVersion)
        {
            if (_calculatingRequests.TryGetValue(hexCoord, out var currentCalculatingVersion) && currentCalculatingVersion == hexVersion)
                _calculatingRequests.Remove(hexCoord);
        }

        public bool Contains(int2 hexCoord) => _awaitingRequests.ContainsKey(hexCoord);

        public void GetAwaitingRequestsList(List<HexUpdateRequest> awaitingRequestsList)
        {
            foreach (var awaitingRequest in _awaitingRequests)
            {
                awaitingRequestsList.Add(new(awaitingRequest.Key, awaitingRequest.Value));
            }
        }
    }
}
