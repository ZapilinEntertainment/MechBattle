using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct CaptureBehaviourComponent : IComponent 
    {
        public readonly CaptureBehaviour Value;

        public CaptureBehaviourComponent(CaptureBehaviour captureBehaviour) => Value = captureBehaviour;
    
    }
}