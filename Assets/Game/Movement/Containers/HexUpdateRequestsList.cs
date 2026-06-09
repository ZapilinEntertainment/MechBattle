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

    public abstract class HexUpdateRequestsList : IEnumerable<HexUpdateRequest>
    {
        public int Count => _updateRequests.Count;
        private readonly Dictionary<int2, int> _updateRequests = new();

        public void AddRequest(int2 hexCoord, int hexVersion)
        {
            if (!_updateRequests.TryGetValue(hexCoord, out var currentRequestedVersion) || hexVersion > currentRequestedVersion)
            {
                _updateRequests[hexCoord] = hexVersion;
            }
        }

        public void RemoveRequest(int2 hexCoord, int updateHexVersion)
        {
            if (_updateRequests.TryGetValue(hexCoord, out var currentRequestVersion) && currentRequestVersion < updateHexVersion)
            {
                _updateRequests.Remove(hexCoord);
            }
        }

        public void RemoveActualRequest(int2 hexCoord) => _updateRequests.Remove(hexCoord);

        public bool Contains(int2 hexCoord) => _updateRequests.ContainsKey(hexCoord);

        protected void Clear() => _updateRequests.Clear();

        #region IEnumerable
        public IEnumerator<HexUpdateRequest> GetEnumerator()
        {
            foreach (var requestKvp in _updateRequests)
            {
                yield return new(requestKvp.Key, requestKvp.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
