using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public abstract class EntityComponentAppearTracker<T> : EntityComponentTrackerBase<T> where T : struct, IComponent
    {
        public EntityComponentAppearTracker(World world) : base(world)
        {
        }

        protected override bool IsStashConditionMatch() => Stash.Has(Entity);
    }
}
