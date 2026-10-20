using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class CaptureFeatureModule : EcsFeatureModule<CaptureSystemsInstallQueue>
    {
        protected override CaptureSystemsInstallQueue CreateQueue() => new();

        private class CaptureDataApplier : INavigationHexCreationApplier
        {
            private readonly Stash<HexCaptureStateComponent> _components;

            [Inject]
            public CaptureDataApplier(World world)
            {
                _components = world.GetStash<HexCaptureStateComponent>();
            }
            public void ApplyOnCreate(Entity hexEntity)
            {
                _components.Add(hexEntity, HexCaptureStateComponent.Empty);
            }
        }

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<INavigationHexCreationApplier, CaptureDataApplier>(Lifetime.Scoped);
        }
    }
}
