using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct CellMovementDensityComponent : IValueComponent<float>
    {
        public float Value { get; set; }

        public CellMovementDensityComponent(float value) => Value = value;
    }
}