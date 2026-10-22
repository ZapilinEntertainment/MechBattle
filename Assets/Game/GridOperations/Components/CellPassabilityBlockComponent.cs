using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct CellPassabilityBlockComponent : IComponent 
    {
        public int PrimaryOccupationTokenId;
        public int SecondaryOccupationTokenId; // if some occupations are overlayed
        public CellOccupationLevelFlags OccupationFlags;

        public CellPassabilityBlockComponent(int tokenId, CellOccupationLevelFlags flags)
        {
            PrimaryOccupationTokenId = tokenId;
            SecondaryOccupationTokenId = -1;
            OccupationFlags = flags;
        }

        public void AddSecondaryToken(int tokenId, CellOccupationLevelFlags secondaryTokenFlags)
        {
            SecondaryOccupationTokenId = tokenId;
            OccupationFlags |= secondaryTokenFlags;
        }
    }
}