using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs
{
    public class TargetSearchFeatureUnitApplier : TargetSearchFeatureApplierBase, IUnitFeatureApplier
    {

        public TargetSearchFeatureUnitApplier(World world) : base(world)
        {
        }

        public void ApplyUnitFeature(Entity entity, IUnitConfig config) =>
            ApplyTargetSearchFeature(entity, config.TargetSearchCooldown, config.TargetSearchRadius, config.MinTargetDistance);
    }
}
