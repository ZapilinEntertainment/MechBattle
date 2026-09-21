using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct CellPassabilityComponent : IValueComponent<CellPassabilityData> 
    {
        public CellPassabilityData Value { get; set; }
    
    }
}