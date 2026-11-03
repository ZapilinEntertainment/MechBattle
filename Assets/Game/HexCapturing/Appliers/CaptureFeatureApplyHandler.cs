using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class CaptureFeatureApplyHandler
    {
        private readonly Stash<CaptureBehaviourComponent> _captureComponents;
        private readonly Stash<CapturableEntityTag> _capturableEntityTags;

        [Inject]
        public CaptureFeatureApplyHandler(World world)
        {
            _captureComponents = world.GetStash<CaptureBehaviourComponent>();
            _capturableEntityTags = world.GetStash<CapturableEntityTag>();
        }

        public void Apply(Entity entity, CaptureBehaviour captureBehaviour)
        {
            _captureComponents.Add(entity, new(captureBehaviour));
            if (captureBehaviour.Type == CaptureBehaviourType.Capturable)
                _capturableEntityTags.Add(entity);
        }
    }
}
