namespace ZE.MechBattle.Navigation
{
    public class PortalExitsMask
    {
        public readonly int[] ExitsMaskA;
        public readonly int[] ExitsMaskB;
        public readonly int[] PortalIdsMask;
        private readonly PortalExitsList _exitsList;
        public const int INVALID_ID = -1;

        public PortalExitsMask(int trianglesPerEdge, PortalExitsList exitsList)
        {
            _exitsList = exitsList;

            var edgeTrisCount = TriangularMath.GetTwoRowEdgeTrianglesCount(trianglesPerEdge);
            ExitsMaskA = new int[edgeTrisCount];
            ExitsMaskB = new int[edgeTrisCount];
            PortalIdsMask = new int[edgeTrisCount];

            Clear();
        }

        public void Clear()
        {
            for (var i = 0; i < ExitsMaskA.Length; i++)
            {
                ExitsMaskA[i] = INVALID_ID;
                ExitsMaskB[i] = INVALID_ID;
                PortalIdsMask[i] = INVALID_ID;
            }
        }

        public void ClearPosition(int index)
        {
            ExitsMaskA[index] = INVALID_ID;
            ExitsMaskB[index] = INVALID_ID;
            PortalIdsMask[index] = INVALID_ID;
        }

        public void WritePortalData(int portalId, NavigationPortal portal)
        {
            AddExit(portal.ExitIdA, ExitsMaskA, portalId);
            AddExit(portal.ExitIdB, ExitsMaskB, portalId);
        }

        public void AddExit(int exitId, int[] mask, int portalId)
        {
            if (!_exitsList.TryGetValue(exitId, out var exitData))
                return;

            for (var i = 0; i < exitData.Length; i++)
            {
                var index = i + exitData.StartTriangleIndex;
                mask[index] = exitId;
                PortalIdsMask[index] = portalId;
            }
        }

        public bool TryGetPortalId(int index, out int portalId) => TryGetCorrectValue(index, PortalIdsMask, out portalId);
        public bool TryGetExitIdA(int index, out int exitIdA) => TryGetCorrectValue(index, ExitsMaskA, out exitIdA);
        public bool TryGetExitIdB(int index, out int exitIdB) => TryGetCorrectValue(index, ExitsMaskB, out exitIdB);

        private bool TryGetCorrectValue(int index, int[] array, out int value)
        {
            value = array[index];
            return value != INVALID_ID;
        }
    }
}
