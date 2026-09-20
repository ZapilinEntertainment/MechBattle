using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct UnitSpawnRequestComponent : IComponent 
    {
        public readonly UnitKey UnitKey;
        public readonly CellPoint CellPoint;
        public readonly PlayerKey PlayerKey;
        public readonly int SquadId;

        public bool SquadAssignmentRequested => SquadId != SQUAD_INVALID_VALUE;
        private const int SQUAD_INVALID_VALUE = -1;

        public UnitSpawnRequestComponent(UnitKey key, CellPoint point, PlayerKey playerKey, int squadId = SQUAD_INVALID_VALUE)
        {
            UnitKey = key;
            PlayerKey = playerKey;
            CellPoint = point;
            SquadId = squadId;
        }
    
    }
}