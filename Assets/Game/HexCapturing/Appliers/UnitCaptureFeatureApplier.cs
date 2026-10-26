using Scellecs.Morpeh;
using System;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    [Serializable]
    public class UnitCaptureFeatureApplier : UnitFeatureApplierBase<UnitCaptureConfig>
    {
        private readonly Stash<CaptureBehaviourComponent> _captureComponents;

        [Inject]
        public UnitCaptureFeatureApplier(World world)
        {
            _captureComponents = world.GetStash<CaptureBehaviourComponent>();
        }

        protected override void ApplyUnitFeature(Entity entity, UnitCaptureConfig config)
        {
            _captureComponents.Add(entity, new(config.CaptureBehaviour));
        }
    }
}
