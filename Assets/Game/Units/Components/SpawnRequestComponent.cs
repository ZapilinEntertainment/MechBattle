using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct SpawnRequestComponent : IComponent 
    {
        public readonly UnitKey UnitKey;
        public readonly IntTriangularPos Tripos;
        public readonly PlayerKey PlayerKey;

        public SpawnRequestComponent(UnitKey key, IntTriangularPos tripos, PlayerKey playerKey)
        {
            UnitKey = key;
            Tripos = tripos;
            PlayerKey = playerKey;
        }
    
    }
}