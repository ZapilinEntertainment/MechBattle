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
            builder.Register<INavigationHexCreationApplier, HexCaptureDataApplier>(Lifetime.Scoped);
            builder.Register<IUnitFeatureApplier, UnitCaptureFeatureApplier>(Lifetime.Scoped);
            builder.Register<IBuildingFeatureApplier, BuildingCaptureFeatureApplier>(Lifetime.Scoped);
        }
    }
}
