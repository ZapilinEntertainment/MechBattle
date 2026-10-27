using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle
{
    public class BuildingCaptureFeatureApplier : BuildingFeatureApplierBase<BuildingCaptureFeatureComponent>
    {
        public override BuildingFunctionFlags RequiredFlagsMask => BuildingFunctionFlags.CaptureResist;
        private readonly CaptureFeatureApplyHandler _applyHandler;
        
 
        [Inject]
        public BuildingCaptureFeatureApplier(CaptureFeatureApplyHandler applyHandler)
        {
            _applyHandler = applyHandler;
        }

        protected override void ApplyBuildingFeature(Entity buildingEntity, BuildingCaptureFeatureComponent captureComponent)
        {
            _applyHandler.Apply(buildingEntity, captureComponent.CaptureBehaviour);
        }
    }
}
