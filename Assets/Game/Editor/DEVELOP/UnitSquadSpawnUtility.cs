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
    public class UnitSquadSpawnUtility : SquadSpawnUtilityBase
    {
        [SerializeField] private bool _spawnByClick = false;
        [Space]
        [SerializeField] private string _unitId = "tank";
        [SerializeField] private int _playerId = 1;
        [SerializeField] private int _spawnRadius = 4;
        [Space]
        [SerializeField] private int2 _startHex;
        [SerializeField] private int2 _endHex;

        [Inject] private CameraController _cameraController;



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
            var hexPos = new NavigationHexPosition(_endHex, _navigationMap);
            _decreeApplier.SetSquadMoveDecree(squadEntity, hexPos.InnerRingTopValleyTriangle);
        }

        private void SpawnUnitSquadAtSelectedHex()
        {
            if (!Physics.Raycast(_cameraController.ScreenPointToRay(Input.mousePosition), out var raycastHit, 1000f, LayerConstants.FootPlacementMask))
                return;

            float3 pos = raycastHit.point;
            var hexCoord = HexMath.DefineHex(pos.xz, _navigationMap.HexEdgeLength);
            SpawnSquadAtHex(hexCoord);
        }

        private Entity SpawnSquadAtHex(int2 hexCoord) =>
            SpawnSquadAtHex(hexCoord, _unitId, new(_playerId), _spawnRadius, TriangularMath.GetTrianglesCountInHex(_spawnRadius));
    }
}
