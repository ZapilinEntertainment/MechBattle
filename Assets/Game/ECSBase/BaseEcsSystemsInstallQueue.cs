using System.Collections.Generic;

namespace ZE.MechBattle.Ecs
{
    public class BaseEcsSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<InitialDelaySystem>(SystemGroupOrder.EarlyUpdate); 

            // ATTENTION: TargetDefineSystem and next TargetValidation are in different systems group
            // because target define system launches a job with World.Handle
            installer.AddSystem<AttackTargetDefineSystem>(SystemGroupOrder.EarlyUpdate);

            installer.AddSystem<AttackTargetValidationSystem>(SystemGroupOrder.Default);
            installer.AddSystem<RestorationSystem>(SystemGroupOrder.Default);

            installer.AddSystem<ProjectileCreateSystem>(SystemGroupOrder.Default);
            installer.AddSystem<DamageCalculationSystem>(SystemGroupOrder.Default);
            installer.AddSystem<DamageApplySystem>(SystemGroupOrder.Default);

            installer.AddSystem<ProjectileMoveSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<ProjectilesExplodeSystem>(SystemGroupOrder.RegularUpdate);

            installer.AddSystem<LocalRotationTargetingSystem>(SystemGroupOrder.TransformUpdates);
            installer.AddSystem<HierarchyTransformUpdateTagSync>(SystemGroupOrder.TransformUpdates);
            installer.AddSystem<ChildPointsUpdateSystem>(SystemGroupOrder.TransformUpdates);
            installer.AddSystem<TransformsSyncSystem>(SystemGroupOrder.TransformUpdates);

            installer.AddSystem<EntityDestructionDelaySystem>(SystemGroupOrder.DisposeTagsSharing);
            installer.AddSystem<HierarchyDisposeSyncSystem>(SystemGroupOrder.DisposeTagsSharing);

            installer.AddSystem<ViewDestroyEffectSystem>(SystemGroupOrder.DisposedObjectsOperations);            
            installer.AddSystem<TransformsClearSystem>(SystemGroupOrder.DisposedObjectsOperations);
            installer.AddSystem<CollidersClearSystem>(SystemGroupOrder.DisposedObjectsOperations);

            installer.AddSystem<LifetimeTrackingSystem>(SystemGroupOrder.Dispose);
            installer.AddSystem<EntityDisposeSystem>(SystemGroupOrder.Dispose);
            installer.AddSystem<UpdateTagsClearSystem>(SystemGroupOrder.Dispose);
        }
    }
}
