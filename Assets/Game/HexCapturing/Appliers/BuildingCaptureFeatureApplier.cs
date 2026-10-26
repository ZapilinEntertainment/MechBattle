using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class BuildingCaptureFeatureApplier : IBuildingFeatureApplier
    {
        public BuildingFunctionFlags RequiredFlagsMask => BuildingFunctionFlags.CaptureResist;
        private readonly Stash<CaptureBehaviourComponent> _captureComponents;

        [Inject]
        public BuildingCaptureFeatureApplier(World world)
        {
            _captureComponents = world.GetStash<CaptureBehaviourComponent>();
        }

        public void ApplyBuildingFeature(Entity buildingEntity, IBuildingView view)
        {
            if (view.GameObject.TryGetComponent<BuildingCaptureFeatureComponent>(out var captureComponent))
            {
                _captureComponents.Add(buildingEntity, new(captureComponent.CaptureBehaviour));
            }
        }
    }
}
