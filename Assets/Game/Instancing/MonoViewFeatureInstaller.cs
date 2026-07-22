using UnityEngine;
using VContainer;
using ZE.MechBattle.Views;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MonoViewFeatureInstaller : EcsFeatureInstaller<MonoViewFeatureSystemsQueue>, ISceneFeatureInitializer
    {
        protected override MonoViewFeatureSystemsQueue CreateQueue() => new();

        public override void OnSceneContainerBuilt(IObjectResolver resolver)
        {
            base.OnSceneContainerBuilt(resolver);
            var world = resolver.Resolve<World>();
            world.GetStash<DisposableViewComponent>().AsDisposable();
        }
    }
}
