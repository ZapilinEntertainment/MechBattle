using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class BuildingCaptureFeatureApplier : BuildingFeatureApplierBase<BuildingCaptureFeatureComponent>
    {
        public override BuildingFunctionFlags RequiredFlagsMask => BuildingFunctionFlags.CaptureResist;
        private readonly Stash<CaptureBehaviourComponent> _captureComponents;

        [Inject]
        public BuildingCaptureFeatureApplier(World world)
        {
            _captureComponents = world.GetStash<CaptureBehaviourComponent>();
        }

        protected override void ApplyBuildingFeature(Entity buildingEntity, BuildingCaptureFeatureComponent captureComponent)
        {
            _captureComponents.Add(buildingEntity, new(captureComponent.CaptureBehaviour));
        }
    }
}
