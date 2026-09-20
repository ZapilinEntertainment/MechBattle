using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using TriInspector;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct SquadComponent : IComponent 
    {
        public readonly int Id;
        public int MembersCount;
#if UNITY_EDITOR
        [ShowInInspector] private readonly int _id => Id;
#endif


        public SquadComponent(int id)
        {
            Id = id;
            MembersCount = 0;
        }
    }
}