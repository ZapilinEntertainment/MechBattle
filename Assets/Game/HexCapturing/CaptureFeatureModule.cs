using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class CaptureFeatureModule : EcsFeatureModule<CaptureSystemsInstallQueue>
    {
        protected override CaptureSystemsInstallQueue CreateQueue() => new();

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<INavigationHexCreationApplier, HexCaptureDataApplier>(Lifetime.Singleton);
            builder.Register<IUnitFeatureApplier, UnitCaptureFeatureApplier>(Lifetime.Singleton);
            builder.Register<IBuildingFeatureApplier, BuildingCaptureFeatureApplier>(Lifetime.Singleton);
            builder.Register<CaptureFeatureApplyHandler>(Lifetime.Singleton);
        }
    }
}
