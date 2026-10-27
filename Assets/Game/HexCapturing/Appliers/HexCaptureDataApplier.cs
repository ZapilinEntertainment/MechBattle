using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class HexCaptureDataApplier : INavigationHexCreationApplier
    {
        private readonly Stash<HexCaptureStateComponent> _components;

        [Inject]
        public HexCaptureDataApplier(World world)
        {
            _components = world.GetStash<HexCaptureStateComponent>();
        }
        public void ApplyOnCreate(Entity hexEntity)
        {
            _components.Add(hexEntity, HexCaptureStateComponent.Empty);
        }
    }
}
