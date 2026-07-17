using VContainer;
using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public class FinalViewFunctionalApplier
    {
        private readonly ColliderOwnityApplier _colliderOwnityApplier;
        private readonly FactionVisibleMarksApplier _factionVisibleMarksApplier;
        

        [Inject]
        public FinalViewFunctionalApplier(ColliderOwnityApplier colliderOwnityApplier, FactionVisibleMarksApplier marksApplier)
        {
            _colliderOwnityApplier = colliderOwnityApplier;
            _factionVisibleMarksApplier = marksApplier;
        }

        public void CheckAndApply(Entity entity, IMonoView view)
        {
            _colliderOwnityApplier.CheckViewForColliders(entity, view);
            _factionVisibleMarksApplier.CheckView(entity, view);
        }
    
    }
}
