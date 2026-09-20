using Unity.Mathematics;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;
using TriInspector;
using Scellecs.Morpeh;
using ZE.MechBattle.Units.Squads;

namespace ZE.MechBattle.Develop
{
    public class UnitSquadSpawnUtility : MonoBehaviour
    {
        [SerializeField] private bool _spawnByClick = false;
        [Space]
        [SerializeField] private string _unitId = "tank";
        [SerializeField] private int _playerId = 1;
        [SerializeField] private int _spawnRadius = 4;
        [Space]
        [SerializeField] private int2 _startHex;
        [SerializeField] private int2 _endHex;

        private CameraController _cameraController;
        private INavigationMap _map;
        private UnitSpawnRequestsFactory _unitSpawnRequestFactory;
        private StringDataDictionary _stringDictionary;

        private SquadFactory _squadFactory;
        private SquadDecreeApplier _decreeApplier;


        [Inject]
        public void Inject(
            CameraController cameraController, 
            INavigationMap map, 
            UnitSpawnRequestsFactory unitSpawnRequestsFactory,
            StringDataDictionary stringDict,
            
            SquadFactory squadFactory,
            SquadDecreeApplier decreeApplier)
        {
            _cameraController = cameraController;
            _map = map;
            _unitSpawnRequestFactory = unitSpawnRequestsFactory;
            _stringDictionary = stringDict;

            _squadFactory = squadFactory;
            _decreeApplier = decreeApplier;
        }


        private void Update()
        {
            if (!_spawnByClick)
                return;

            if (Input.GetMouseButtonDown(0))
                SpawnUnitSquadAtSelectedHex();
        }

        [Button]
        private void SpawnAndCommand()
        {
            var squadEntity = SpawnSquadAtHex(_startHex);
            var hexPos = new NavigationHexPosition(_endHex, _map);
            _decreeApplier.SetSquadMoveDecree(squadEntity, hexPos.InnerRingTopValleyTriangle);
        }

        private void SpawnUnitSquadAtSelectedHex()
        {
            if (!Physics.Raycast(_cameraController.ScreenPointToRay(Input.mousePosition), out var raycastHit, 1000f, LayerConstants.FootPlacementMask))
                return;

            float3 pos = raycastHit.point;
            var hexCoord = HexMath.DefineHex(pos.xz, _map.HexEdgeLength);
            SpawnSquadAtHex(hexCoord);
        }

        private Entity SpawnSquadAtHex(int2 hexCoord)
        {
            var hexPos = new NavigationHexPosition(hexCoord, _map.HexEdgeLength, _map.TrianglesPerHexEdge);

            var unitKey = new UnitKey(_stringDictionary.StringToKey(_unitId));
            var playerKey = new PlayerKey(_playerId);
            var count = 0;
            var (squadEntity, squadId) = _squadFactory.Create();

            foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, _spawnRadius))
            {
                _unitSpawnRequestFactory.CreateSpawnRequest(unitKey, tripos, playerKey, squadId);
                count++;
            }

            UnityEngine.Debug.Log($"requested {count} units at {hexCoord}");

            return squadEntity;
        }
    }
}
