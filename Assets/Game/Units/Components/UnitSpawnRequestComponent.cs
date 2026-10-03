using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct UnitSpawnRequestComponent : IRequestComponent 
    {
        public readonly UnitKey UnitKey;
        public readonly CellPoint CellPoint;
        public readonly PlayerKey PlayerKey;
        public readonly Entity SquadEntity;

        public UnitSpawnRequestComponent(UnitKey key, CellPoint point, PlayerKey playerKey, Entity squadEntity = default)
        {
            UnitKey = key;
            PlayerKey = playerKey;
            CellPoint = point;
            SquadEntity = squadEntity;
        }
    
    }
}