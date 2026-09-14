using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {

    public enum EnergyGridMode : byte { Undefined, Partitions}

    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct EnergyGridModeComponent : IComponent 
    {
        public readonly EnergyGridMode Mode;
        public readonly Entity Entity;

        private EnergyGridModeComponent(EnergyGridMode mode, Entity entity)
        {
            Mode = mode;
            Entity = entity;
        }

        public static EnergyGridModeComponent CreatePartitionsModeComponent(Entity mechEntity) => new(EnergyGridMode.Partitions, mechEntity);

    }


}