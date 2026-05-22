using Unity.Mathematics;

namespace ZE.MechBattle
{
    public readonly struct NavigationPortal
    {
        public readonly int Id;
        public readonly NavigationPortalExit ExitA;
        public readonly NavigationPortalExit ExitB;
    
        public NavigationPortal(int id, NavigationPortalExit exitA, NavigationPortalExit exitB)
        {
            Id = id; 
            ExitA = exitA; 
            ExitB = exitB;
        }

        public NavigationPortalExit GetExit(int2 hexCoord) => math.all(hexCoord == ExitA.HexCoord) ? ExitA : ExitB;
    }
}
