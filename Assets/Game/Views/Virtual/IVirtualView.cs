using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public interface IVirtualView : IView
    {
        VirtualViewComponent ViewData { get; }   
    }
}
