using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct SpawnOperationsLeftComponent : IComponent 
    {
        public int Value;
        public SpawnLimitExhaustedStrategy Strategy;

    }

    public enum SpawnLimitExhaustedStrategy : byte
    {
        Dispose,
        StopSpawn
    }
}