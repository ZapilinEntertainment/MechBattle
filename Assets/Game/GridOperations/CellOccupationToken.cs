using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public readonly struct CellOccupationToken
    {
        public readonly int Key;
        public readonly CellOccupationLevelFlags OccupationFlags;

        public CellOccupationToken(int key, CellOccupationLevelFlags occupationLevelFlags)
        {
            Key = key;
            OccupationFlags = occupationLevelFlags;
        }

    }
}
