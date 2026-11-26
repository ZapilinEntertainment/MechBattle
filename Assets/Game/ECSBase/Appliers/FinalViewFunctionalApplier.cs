using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class FinalViewFunctionalApplier
    {
        private readonly ColliderOwnityApplier _colliderOwnityApplier;
        private readonly Stash<PlayerAffinityChangedTag> _playerAffinityChangedTags;
        private readonly Stash<PlayerAffiliationComponent> _playerAffiliationComponents;
        private readonly Stash<FactionColorApplicableTag> _factionColorApplicableTags;
        

        [Inject]
        public FinalViewFunctionalApplier(ColliderOwnityApplier colliderOwnityApplier, World world)
        {
            _colliderOwnityApplier = colliderOwnityApplier;
            _playerAffinityChangedTags = world.GetStash<PlayerAffinityChangedTag>();
            _playerAffiliationComponents = world.GetStash<PlayerAffiliationComponent>();
            _factionColorApplicableTags = world.GetStash<FactionColorApplicableTag>();
        }

        public void CheckMonoViewAndApply(Entity entity, IMonoView view)
        {
            _colliderOwnityApplier.CheckViewForColliders(entity, view);
            _playerAffinityChangedTags.Set(entity);
        }

        public void CheckVirtualViewAndApply(Entity mainEntity, Entity viewEntity)
        {
            // todo: collider handling
            if (_factionColorApplicableTags.Has(viewEntity))
            {
                var affiliation = _playerAffiliationComponents.Get(mainEntity, out var haveAffiliation);
                SyncComponentsCommand.Execute(viewEntity, mainEntity, _playerAffiliationComponents);
                //_playerAffinityChangedTags.Set(mainEntity); - done in calling method
            }           
        }
    }
}
