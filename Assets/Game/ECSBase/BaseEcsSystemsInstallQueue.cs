using System.Collections.Generic;

namespace ZE.MechBattle.Ecs
{
    public class BaseEcsSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddInitializer<WorldInitializer>(SystemGroupOrder.Initialization);
            installer.AddInitializer<DamageablesInitializer>(SystemGroupOrder.Initialization);
            installer.AddInitializer<SceneUnitsInitializer>(SystemGroupOrder.Initialization);

            installer.AddSystem<InitialDelaySystem>(SystemGroupOrder.Initialization);  

            installer.AddSystem<AttackTargetDefineSystem>(SystemGroupOrder.Default);
            installer.AddSystem<AttackTargetValidationSystem>(SystemGroupOrder.Default);

            installer.AddSystem<ViewRequestsHandleSystem>(SystemGroupOrder.Default);
            installer.AddSystem<VfxCreateSystem>(SystemGroupOrder.Default);
            installer.AddSystem<RestorationSystem>(SystemGroupOrder.Default);

            installer.AddSystem<ProjectileCreateSystem>(SystemGroupOrder.Default);
            installer.AddSystem<DamageCalculationSystem>(SystemGroupOrder.Default);
            installer.AddSystem<DamageApplySystem>(SystemGroupOrder.Default);

            installer.AddSystem<ProjectileMoveSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<ProjectilesExplodeSystem>(SystemGroupOrder.RegularUpdate);

            installer.AddSystem<TransformsSyncSystem>(SystemGroupOrder.PostUpdate);
            installer.AddSystem<ViewDestroyEffectSystem>(SystemGroupOrder.PostUpdate);

            installer.AddSystem<EntityDestructionDelaySystem>(SystemGroupOrder.Final);

            installer.AddSystem<TransformsClearSystem>(SystemGroupOrder.Final);
            installer.AddSystem<CollidersClearSystem>(SystemGroupOrder.Final);
            installer.AddSystem<EntityDisposeSystem>(SystemGroupOrder.Final);
            installer.AddSystem<UpdateTagsClearSystem>(SystemGroupOrder.Final);
        }
    }
}
