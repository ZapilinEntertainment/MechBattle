using ZE.MechBattle.Views;

namespace ZE.MechBattle
{

    public interface IViewContainersManager
    {
        (MonoViewContainer container, int id) GetMonoContainer();
        (VirtualViewContainer container, int id) GetVirtualContainer();
        bool TryGetContainer(int id, out IViewContainer container);
        void Release(int id);

        int RegisterOneShotContainer(IViewContainer viewContainer);
    }
}
