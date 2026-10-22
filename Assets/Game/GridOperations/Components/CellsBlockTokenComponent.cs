using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]

    // add to entity, that is obstacle
    public readonly struct CellsBlockTokenComponent : IComponent 
    {
        public readonly int TokenId;
        public CellsBlockTokenComponent(int tokenId) => TokenId = tokenId;

    }
}