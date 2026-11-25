using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class FinalViewFunctionalApplier
    {
        private readonly ColliderOwnityApplier _colliderOwnityApplier;
        private readonly Stash<PlayerAffinityChangedTag> _playerAffinityChangedTags;
        

        [Inject]
        public FinalViewFunctionalApplier(ColliderOwnityApplier colliderOwnityApplier, World world)
        {
            _colliderOwnityApplier = colliderOwnityApplier;
            _playerAffinityChangedTags = world.GetStash<PlayerAffinityChangedTag>();
        }

        public void CheckAndApply(Entity entity, IMonoView view)
        {
            _colliderOwnityApplier.CheckViewForColliders(entity, view);
            _playerAffinityChangedTags.Set(entity);
        }
    
    }
}
