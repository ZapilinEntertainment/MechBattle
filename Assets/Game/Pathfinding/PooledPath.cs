using Pathfinding;

namespace ZE.MechBattle.Ecs.Pathfinding
{
    public readonly struct PooledPath
    {
        public readonly ABPath Path;
        public readonly object Holder;   
        
        public PooledPath(ABPath path, object holder)
        {
            Path = path; 
            Holder = holder;
        }
    }
}
