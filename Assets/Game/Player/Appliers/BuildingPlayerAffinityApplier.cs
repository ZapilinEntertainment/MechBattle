using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class BuildingPlayerAffinityApplier : BuildingFeatureApplierBase<PlayerAffinityMonoComponent>
    {
        public override BuildingFunctionFlags RequiredFlagsMask => BuildingFunctionFlags.PlayerAffinity;
        private readonly Stash<PlayerAffiliationComponent> _playerAffiliations;

        [Inject]
        public BuildingPlayerAffinityApplier(World world)
        {
            _playerAffiliations = world.GetStash<PlayerAffiliationComponent>();
        }

        protected override void ApplyBuildingFeature(Entity entity, PlayerAffinityMonoComponent monoComponent)
        {
            _playerAffiliations.Add(entity, new(monoComponent.PlayerKey));
        }
    }
}
