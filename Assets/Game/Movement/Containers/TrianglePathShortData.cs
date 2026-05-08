namespace ZE.MechBattle
{
    public readonly struct TrianglePathShortData
    {
        public readonly int PathId;
        public readonly int TrianglesCount;

        public TrianglePathShortData(int pathId, int trianglesCount)
        {
            PathId = pathId;
            TrianglesCount = trianglesCount;
        }
    }
}
