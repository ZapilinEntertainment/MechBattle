using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {

    public enum SquadDecreeType : byte { Undefined, Move}

    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct SquadDecreeComponent : IComponent 
    {
        public SquadDecreeType Type;
    }
}