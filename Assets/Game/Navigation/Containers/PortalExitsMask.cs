namespace ZE.MechBattle.Navigation
{
    public class PortalExitsMask
    {
        public readonly int Length;
        private readonly int[] ExitsMaskA;
        private readonly int[] ExitsMaskB;
        private readonly int[] PortalIds;
        private readonly PortalExitsList _exitsList;        
        public const int INVALID_ID = -1;

        public PortalExitsMask(int trianglesPerEdge, PortalExitsList exitsList)
        {
            _exitsList = exitsList;

            Length = TriangularMath.GetTwoRowEdgeTrianglesCount(trianglesPerEdge);
            ExitsMaskA = new int[Length];
            ExitsMaskB = new int[Length];
            PortalIds = new int[Length];

            Clear();
        }

        public void Clear()
        {
            for (var i = 0; i < ExitsMaskA.Length; i++)
            {
                ClearPosition(i);
            }
        }

        public void ClearPosition(int index)
        {
            ExitsMaskA[index] = INVALID_ID;
            ExitsMaskB[index] = INVALID_ID;
            PortalIds[index] = INVALID_ID;
        }

        public (int exitIdA, int exitIdB) GetPairExits(int indexA) => (ExitsMaskA[indexA], ExitsMaskB[ReverseIndex(indexA)]);

        public void SetPortalId(int indexA, int portalId)
        {
            PortalIds[indexA] = portalId;
        }

        public void WritePortalData(int portalId, NavigationPortal portal)
        {
            AddExit(portal.ExitIdA, portalId, sideA: true);
            AddExit(portal.ExitIdB, portalId, sideA: false);
        }

        public void AddExit(int exitId, int portalId, bool sideA)
        {
            if (!_exitsList.TryGetValue(exitId, out var exitData))
                return;


            var exitMask = sideA ? ExitsMaskA : ExitsMaskB;

            for (var i = 0; i < exitData.Length; i++)
            {
                var index = i + exitData.StartTriangleIndex;
                exitMask[index] = exitId;

                var portalIndex = sideA ? index : ReverseIndex(index);
                PortalIds[portalIndex] = portalId;
            }
        }

        public bool TryGetPortalId(int indexA, out int portalId) => TryGetCorrectValue(indexA, PortalIds, out portalId);
        public bool TryGetExitIdA(int indexA, out int exitIdA) => TryGetCorrectValue(indexA, ExitsMaskA, out exitIdA);
        public bool TryGetExitIdB(int indexA, out int exitIdB) => TryGetCorrectValue(ReverseIndex(indexA), ExitsMaskB, out exitIdB);

        public int GetExitIdA(int indexA) => ExitsMaskA[indexA];
        public int GetExitIdB(int indexA) => ExitsMaskB[ReverseIndex(indexA)];
        public int GetPortalId(int indexA) => PortalIds[indexA];

        private bool TryGetCorrectValue(int index, int[] array, out int value)
        {
            value = array[index];
            return value != INVALID_ID;
        }

        private int ReverseIndex(int index) => Length - 1 - index;
    }
}
