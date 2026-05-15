using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public readonly struct HexPathOption
    {
        public readonly bool IsValid;
        public readonly int PathId;
        public readonly float RawPathCost;
        public readonly float FullPathCost => RawPathCost + StartEdgeCost + EndEdgeCost;
        public readonly float StartEdgeCost;
        public readonly float EndEdgeCost;
        public readonly PathData<HexPathNodeKey> PathData;
        public HexPathNodeKey LastNode => PathData.LastNode;
        public int NodesCount => PathData.NodesCount;

        public HexPathOption(int pathId, PathData<HexPathNodeKey> pathData, float startEdgeCost, float endEdgeCost)
        {
            IsValid = true;
            PathId = pathId;
            PathData = pathData;

            StartEdgeCost = startEdgeCost;
            EndEdgeCost = endEdgeCost;
            RawPathCost = pathData.PathCost;
        }

        private HexPathOption(bool isValid)
        {
            IsValid = isValid;
            PathId = -1;
            PathData = null;
            
            StartEdgeCost = 0;
            EndEdgeCost = 0;
            RawPathCost = float.MaxValue;
        }

        public static HexPathOption Default => new(false);
    }
}
