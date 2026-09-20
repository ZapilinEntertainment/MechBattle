using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using TriInspector;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct SquadMemberComponent : IComponent 
    {
        public readonly int SquadId;
        public int Index;
#if UNITY_EDITOR
        [ShowInInspector] private int _squadId => SquadId;
#endif

        public SquadMemberComponent(int squadId, int index)
        {
            SquadId = squadId;
            Index = index;
        }
    
    }
}