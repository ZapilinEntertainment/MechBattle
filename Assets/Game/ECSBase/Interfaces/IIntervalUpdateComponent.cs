using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs
{
    public interface IIntervalUpdateComponent : IComponent
    {
        float TimeLeft { get; set;   }
        float Interval { get;}
    
    }
}
