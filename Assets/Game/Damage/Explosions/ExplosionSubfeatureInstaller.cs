using VContainer;
using ZE.MechBattle.Ecs;
using static ZE.MechBattle.Ecs.FeatureSystemsInstallQueue;

namespace ZE.MechBattle.Damage
{
    public static class ExplosionSubfeatureInstaller
    {
        public static void OnSceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<ExplosionRequestsBuilder>(Lifetime.Singleton);
        }

        public static void InstallSystems(ISystemsOperator installer)
        {
            installer.AddSystem<ExplosionCellAffectionSystem>(SystemGroupOrder.DamageCalculation-1);
            installer.AddSystem<ExplosionDamageApplySystem>(SystemGroupOrder.DamageCalculation-1);
        }
    
    }
}
