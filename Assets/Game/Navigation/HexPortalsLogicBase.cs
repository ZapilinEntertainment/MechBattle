using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public interface IPortalsLogic
    {
        void OnPortalOutdated(int id);
        void ApplyPortalDistancesMap(CalculatePointDistancesResults results);
        void RemovePortal(int portalId);
        void GetHexPortalExits(int zoneIndex, int2 hexCoord, ICollection<HexExitOption> exits);
        int RegisterNewPortal(NavigationPortal portal);
        float3 GetPortalCenterTriangular(int portalId);

    }

    public class HexPortalsLogicBase : IPortalsLogic
    {
        protected readonly struct EnumerationResult
        {
            public readonly int PortalId;
            public readonly NavigationPortal Portal;
            public readonly bool MatchedSideA;

            public EnumerationResult(int portalId, NavigationPortal portal, bool matchedSideA)
            {
                PortalId = portalId;
                Portal = portal;
                MatchedSideA = matchedSideA;
            }
        }

        protected readonly HexPortalsList _portals;
        protected readonly IExitsLogic _exitsLogic;
        protected readonly IPortalExitsList _exitsList;
        protected readonly PortalConnectionsList _connectionsList;        
        private readonly List<(int portalId, int exitId, NavigationPortalExit exit)> _cachedPointsList = new();

        public HexPortalsLogicBase(
            HexPortalsList portals,            
            PortalConnectionsList connectionsList,
            IExitsLogic exitsLogic,
            IPortalExitsList exitsList)
        {
            _portals = portals;
            _exitsLogic = exitsLogic;
            _exitsList = exitsList;
            _connectionsList = connectionsList;
        }

        public virtual int RegisterNewPortal(NavigationPortal portal) => _portals.RegisterNewPortal(portal);

        public virtual void OnPortalOutdated(int id) { }

        public void RemovePortal(int portalId)
        {
            _portals.Remove(portalId);
            _connectionsList.RemoveConnection(portalId);
        }

        public void GetHexPortalExits(int zoneIndex, int2 hexCoord, ICollection<HexExitOption> exits)
        {
            foreach (var result in EnumerateHexPortals(hexCoord))
            {
                var portalData = result.Portal;
                var exitId = result.MatchedSideA ? portalData.ExitIdA : portalData.ExitIdB;
                if (_exitsLogic.TryGetExitDataWithValidation(exitId, out var exitData) && exitData.ZoneIndex == zoneIndex)
                    exits.Add(new(result.PortalId, exitId, exitData));
            }
        }

        public void ApplyPortalDistancesMap(CalculatePointDistancesResults results)
        {
            //prepare all exits data in hex (except selected one)
            var hexCoord = results.HexCoord;

            foreach (var enumerationData in EnumerateHexPortals(hexCoord))
            {
                if (enumerationData.PortalId == results.PortalId)
                    continue;

                var portalData = enumerationData.Portal;
                var exitId = enumerationData.MatchedSideA ? portalData.ExitIdA : portalData.ExitIdB;
                if (!_exitsList.TryGetValue(exitId, out var exitData))
                    continue;

                _cachedPointsList.Add((
                    enumerationData.PortalId,
                    exitId,
                    exitData
                    ));
            }

            var count = _cachedPointsList.Count;
            if (count < 2)
                return;

            // calculate distances and write them
            for (var i = 0; i < count; i++)
            {
                var pointData = _cachedPointsList[i];
                if (!results.TryGetDistance(pointData.exit.Center, out var distance))
                    continue;

                _connectionsList.AddConnection(results.PortalId, pointData.portalId, distance);
            }

            // clear cache list
            _cachedPointsList.Clear();
        }

        public float3 GetPortalCenterTriangular(int portalId)
        {
            var portal = _portals[portalId];
            var exitCenterA = _exitsList[portal.ExitIdA].Center.ToFloat3();
            var exitCenterB = _exitsList[portal.ExitIdB].Center.ToFloat3();
            return math.lerp(exitCenterA, exitCenterB, 0.5f);
        }

        private IEnumerable<EnumerationResult> EnumerateHexPortals(int2 hexCoord)
        {
            var portalsCount = _portals.Count;
            if (portalsCount == 0)
                yield break;

            var hexCoordMatchValue = new int4(hexCoord, hexCoord);
            foreach (var portalKvp in _portals)
            {
                var portal = portalKvp.Value;
                var match = hexCoordMatchValue == new int4(portal.HexCoordA, portal.HexCoordB);
                var isCoordA = match.x & match.y;
                var isCoordB = match.z & match.w;
                if (isCoordA | isCoordB)
                    yield return new(portalKvp.Key, portal, isCoordA);
            }
        }


    }
}
