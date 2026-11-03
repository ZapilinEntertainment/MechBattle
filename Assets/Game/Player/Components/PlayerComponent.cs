using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct PlayerComponent : IComponent 
    {
        public readonly PlayerKey PlayerKey;

        public PlayerComponent(PlayerKey key) => PlayerKey = key;

    }
}