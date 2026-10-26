using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using TriInspector;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct CaptureBehaviourComponent : IComponent 
    {
        [ShowInInspector, ReadOnly] public readonly CaptureBehaviour Value;

        public CaptureBehaviourComponent(CaptureBehaviour captureBehaviour) => Value = captureBehaviour;
    
    }
}