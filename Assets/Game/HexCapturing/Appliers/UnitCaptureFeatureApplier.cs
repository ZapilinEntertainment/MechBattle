using Scellecs.Morpeh;
using System;
using VContainer;
using ZE.MechBattle.Units;

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

        public override void ApplyUnitFeature(Entity entity, UnitCaptureConfig config)
        {
            _applyHandler.Apply(entity, config.CaptureBehaviour);
        }

        public override bool TryCacheApplier(IUnitConfig config, out ICachedApplier position)
        {
            if (config.TryGetFeaturedConfig<UnitCaptureConfig>(out var captureConfig))
            {
                position = new SpecificatedConfigCachedApplier<UnitCaptureFeatureApplier, UnitCaptureConfig>(this, captureConfig);
                return true;
            }
            else
            {
                position = default;
                return false;
            }
        }
    }
}
