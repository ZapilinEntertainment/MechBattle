using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Units.Squads;

namespace ZE.MechBattle.Develop
{
    public abstract class SquadSpawnUtilityBase : MonoBehaviour
    {
        protected INavigationMap _navigationMap;
        protected UnitSpawnRequestsFactory _unitSpawnRequestFactory;
        protected StringDataDictionary _stringDictionary;

        protected SquadFactory _squadFactory;
        protected SquadDecreeApplier _decreeApplier;

        [Inject]
        public void Inject(
         INavigationMap navigationMap,
         UnitSpawnRequestsFactory unitSpawnRequestsFactory,
         StringDataDictionary stringDict,

         SquadFactory squadFactory,
         SquadDecreeApplier decreeApplier)
        {
            _navigationMap = navigationMap;
            _unitSpawnRequestFactory = unitSpawnRequestsFactory;
            _stringDictionary = stringDict;

            _squadFactory = squadFactory;
            _decreeApplier = decreeApplier;
        }

        protected Entity SpawnSquadAtHex(int2 hexCoord, string unitId, PlayerKey playerKey, int spawnRadius, int spawnCount)
        {
            var hexPos = new NavigationHexPosition(hexCoord, _navigationMap.HexEdgeLength, _navigationMap.TrianglesPerHexEdge);

            var unitKey = new UnitKey(_stringDictionary.StringToKey(unitId));
            var count = 0;
            var (squadEntity, squadId) = _squadFactory.Create();
            
            foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, spawnRadius))
            {
                _unitSpawnRequestFactory.CreateSpawnRequest(unitKey, tripos, playerKey, squadId);
                count++;
                if (count == spawnCount)
                    break;
            }

            UnityEngine.Debug.Log($"requested {count} units at {hexCoord}");

            return squadEntity;
        }

    }
}
