using ZE.MechBattle.Views;

namespace ZE.MechBattle
{

    public interface IViewContainersList
    {
        bool TryGetContainer(int id, out IViewContainer container);
    }
}
