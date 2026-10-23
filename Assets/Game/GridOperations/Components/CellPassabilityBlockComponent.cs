using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct CellPassabilityBlockComponent : IComponent 
    {
        public Entity PrimaryOccupationEntity;
        public Entity SecondaryOccupationEntity; // if some occupations are overlayed
        public CellOccupationLevelFlags OccupationFlags;

        public CellPassabilityBlockComponent(Entity entity, CellOccupationLevelFlags flags)
        {
            PrimaryOccupationEntity = entity;
            SecondaryOccupationEntity = default;
            OccupationFlags = flags;
        }

        public void AddSecondaryToken(Entity entity, CellOccupationLevelFlags secondaryTokenFlags)
        {
            SecondaryOccupationEntity = entity;
            OccupationFlags |= secondaryTokenFlags;
        }
    }
}