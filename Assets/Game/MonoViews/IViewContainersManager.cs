using ZE.MechBattle.Views;

namespace ZE.MechBattle
{

    public interface IViewContainersManager
    {
        (ViewContainer container, int id) Get();
        bool TryGetContainer(int id, out IViewContainer container);
        void Release(int id);
    }
}
