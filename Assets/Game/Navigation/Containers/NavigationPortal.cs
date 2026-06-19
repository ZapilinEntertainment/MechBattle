using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct NavigationPortal
    {
        public readonly int2 HexCoordA;
        public readonly int2 HexCoordB;
        public readonly int ExitIdA;
        public readonly int ExitIdB;
    
        public NavigationPortal(int exitIdA, int2 hexCoordA, int exitIdB, int2 hexCoordB)
        {
            ExitIdA = exitIdA;
            ExitIdB = exitIdB;
            HexCoordA = hexCoordA;
            HexCoordB = hexCoordB;
        }

        public override string ToString() => $"exit A: {ExitIdA} at {HexCoordA}; exit B: {ExitIdB} at {HexCoordB}";
    }
}
