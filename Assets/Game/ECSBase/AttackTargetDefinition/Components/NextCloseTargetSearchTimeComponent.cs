using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct NextCloseTargetSearchTimeComponent : IDelayComponent
    {
        public float StopTime { get; private set; }

        public NextCloseTargetSearchTimeComponent(float value)
        {
            StopTime = value;
        }

    }
}