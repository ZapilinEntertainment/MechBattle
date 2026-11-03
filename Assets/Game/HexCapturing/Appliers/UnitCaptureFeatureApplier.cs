using Scellecs.Morpeh;
using System;
using VContainer;

namespace ZE.MechBattle
{
    [Serializable]
    public class UnitCaptureFeatureApplier : UnitFeatureApplierBase<UnitCaptureConfig>
    {
        private readonly CaptureFeatureApplyHandler _applyHandler;


        [Inject]
        public UnitCaptureFeatureApplier(CaptureFeatureApplyHandler applyHandler)
        {
            _applyHandler = applyHandler;
        }

        protected override void ApplyUnitFeature(Entity entity, UnitCaptureConfig config)
        {
            _applyHandler.Apply(entity, config.CaptureBehaviour);
        }
    }
}
