namespace ZE.MechBattle.Ecs
{
    public class BaseEcsSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<InitialDelaySystem>(SystemGroupOrder.EarlyUpdate); 

            // ATTENTION: TargetDefineSystem and next TargetValidation are in different systems group
            // because target define system launches a job with World.Handle
            //installer.AddSystem<AttackTargetDefineSystem>(SystemGroupOrder.EarlyUpdate);

            installer.AddSystem<AttackTargetValidationSystem>(SystemGroupOrder.Default);
            installer.AddSystem<AttackTargetSpecificationSystem>(SystemGroupOrder.Default);
            installer.AddSystem<CompositeTargetClearTagSystem>(SystemGroupOrder.AfterDispose);

            installer.AddSystem<RestorationSystem>(SystemGroupOrder.Default);
            installer.AddSystem<ProjectileCreateSystem>(SystemGroupOrder.Default);            

            installer.AddSystem<ProjectileMoveSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<ProjectilesExplodeSystem>(SystemGroupOrder.RegularUpdate);

            // requires job completion
            installer.AddSystem<LocalRotationTargetingSystem>(SystemGroupOrder.TransformUpdates1);

            installer.AddSystem<HierarchyTransformUpdateTagSync>(SystemGroupOrder.TransformUpdates2);
            installer.AddSystem<ChildPointsUpdateSystem>(SystemGroupOrder.TransformUpdates2);
            installer.AddSystem<TransformsSyncSystem>(SystemGroupOrder.TransformUpdates2);

            installer.AddSystem<EntityDestructionDelaySystem>(SystemGroupOrder.DisposeTagsSharing);
            installer.AddSystem<HierarchyDisposeSyncSystem>(SystemGroupOrder.DisposeTagsSharing);

            installer.AddSystem<ViewDestroyEffectSystem>(SystemGroupOrder.DisposedObjectsOperations);            
            installer.AddSystem<TransformsClearSystem>(SystemGroupOrder.DisposedObjectsOperations);     

            installer.AddSystem<LifetimeTrackingSystem>(SystemGroupOrder.Dispose);
            installer.AddSystem<EntityDisposeSystem>(SystemGroupOrder.Dispose);
            installer.AddSystem<UpdateTagsClearSystem>(SystemGroupOrder.Dispose);
        }
    }
}
