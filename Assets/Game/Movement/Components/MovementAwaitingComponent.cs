using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct MovementAwaitingComponent : IComponent 
    {
        public readonly AwaitingToken Token;
        public MovementAwaitingComponent (AwaitingToken token)
        {
            Token = token;
        }
    
    }
}