using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public readonly struct HexPathOption
    {
        public readonly bool IsValid;
        public readonly int PathId;
        public readonly float PathCost;
        public readonly PathData<HexPathNodeKey> PathData;
        public HexPathNodeKey LastNode => PathData.LastNode;
        public int NodesCount => PathData.NodesCount;

        public HexPathOption(int pathId, PathData<HexPathNodeKey> pathData)
        {
            IsValid = true;
            PathId = pathId;
            PathData = pathData;
            PathCost = pathData.PathCost;
        }

        private HexPathOption(bool isValid)
        {
            IsValid = isValid;
            PathId = -1;
            PathData = null;
            PathCost = float.MaxValue;
        }

        public static HexPathOption Default => new(false);
    }
}
