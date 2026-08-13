using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public abstract class EntityComponentRemoveTracker<T> : EntityComponentTrackerBase<T> where T : struct, IComponent
    {
        public EntityComponentRemoveTracker(World world) : base(world)
        {
        }

        protected override bool IsStashConditionMatch() => !Stash.Has(Entity);
    }
}
