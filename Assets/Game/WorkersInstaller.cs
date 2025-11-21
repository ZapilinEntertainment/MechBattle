using ZE.Workers;
using ZE.MechBattle.UI;
using VContainer;
using VContainer.Unity;

namespace ZE.MechBattle
{
    public static class WorkersInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            // IMPORTANT: do not use AsImplementedInterfaces() for ITickable - it will double every instance on resolve
            void RegisterWorker<T>() where T : Worker => builder.Register<T>(Lifetime.Transient);

            RegisterWorker<AimWorker>();
            RegisterWorker<PlayerInterfaceWorker>();
        }            
    }
}
