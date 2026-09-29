using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using ZE.MechBattle.Ecs;
using TriInspector;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Develop
{
    public class SquadsMassSpawnUtility : SquadSpawnUtilityBase
    {
        [Serializable]
        public struct SquadSpawnData 
        {
            public bool IsActive;

            public int UnitsCount;
            public string UnitId;
            public int PlayerId;
            public SquadDecreeType SquadDecree;
            public int2 SpawnHex;
            public int2 TargetHex;

            public int SpawnRadius => HexMath.GetMinHexRadius(UnitsCount);
        }

        [SerializeField] private List<SquadSpawnData> _setups;


        [Button]
        private void SpawnAndCommand()
        {
            foreach (var setup in _setups)
            {
                if (!setup.IsActive)
                    continue;

                var squadEntity = SpawnSquadAtHex(setup.SpawnHex, setup.UnitId, new(setup.PlayerId), setup.SpawnRadius, setup.UnitsCount);

                if (setup.SquadDecree == SquadDecreeType.Move)
                {
                    var hexPos = new NavigationHexPosition(setup.TargetHex, _navigationMap);
                    _decreeApplier.SetSquadMoveDecree(squadEntity, hexPos.InnerRingTopValleyTriangle);
                }
            }            
        }
    }
}
