using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using TriInspector;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct HexComponent : IComponent 
    {
        // for hex entities. For simple hex coord use HexCoordComponent

        [ShowInInspector, ReadOnly] public readonly int2 Coord;

        public HexComponent(int2 coord) => Coord = coord;
    
    }
}