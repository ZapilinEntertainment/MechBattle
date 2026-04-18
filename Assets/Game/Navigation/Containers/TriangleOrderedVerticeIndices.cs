namespace ZE.MechBattle.Navigation
{
    public readonly struct TriangleOrderedVerticeIndices
    {
        //  LB---RB       Pinnacle
        //   \   /         /   \
        //   Pinnacle     LB---RB

        public readonly int PinnacleIndex;
        public readonly int LeftBasisIndex;
        public readonly int RightBasisIndex;

        public TriangleOrderedVerticeIndices(int pinnacleIndex, int leftBasisIndex, int rightBasisIndex)
        {
            PinnacleIndex = pinnacleIndex;
            LeftBasisIndex = leftBasisIndex;
            RightBasisIndex = rightBasisIndex;
        }
    
    }
}
