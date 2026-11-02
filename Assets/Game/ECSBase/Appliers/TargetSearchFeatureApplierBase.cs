using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs
{
    public class TargetSearchFeatureApplierBase
    {
        private readonly Stash<TargetSearchRadiusComponent> _targetSearchRadius;
        private readonly Stash<ClosestTargetSearchCooldownComponent> _searchCooldown;

        public TargetSearchFeatureApplierBase(World world)
        {
            _targetSearchRadius = world.GetStash<TargetSearchRadiusComponent>();
            _searchCooldown = world.GetStash<ClosestTargetSearchCooldownComponent>();
        }

        public void ApplyTargetSearchFeature(Entity entity, float searchCd, float maxSearchRadius,  float minSearchRadius = 0f)
        {
            if (maxSearchRadius != 0f)
                _targetSearchRadius.Add(entity, new(maxSearchRadius, minSearchRadius));

            _searchCooldown.Add(entity, new(searchCd));
        }
    }
}
