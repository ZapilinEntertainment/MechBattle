using ZE.MechBattle.Views;

namespace ZE.MechBattle
{

    public interface IViewContainersPool
    {
        bool TryGetContainer(int id, out IViewContainer container);
        void Release(int id);
    }
}
