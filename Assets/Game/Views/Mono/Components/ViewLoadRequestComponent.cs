using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct ViewLoadRequestComponent : IComponent 
    {
        public readonly int EncodedAssetNameId;

        public ViewLoadRequestComponent(int encodedId) => EncodedAssetNameId = encodedId;
    }
}