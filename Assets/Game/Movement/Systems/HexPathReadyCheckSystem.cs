using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPathReadyCheckSystem : PathReadyCheckSystemBase<
        HexPathProcessingTag, 
        HexPathSearchRequestComponent,
        HexPathCalculationRequestTag,
        HexPathReadyTag,
        ClearHexPathTag> 
    {
        
    }
}