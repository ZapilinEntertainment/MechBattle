using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle
{
    public abstract class HexCreationComponentApplier<T> : INavigationHexCreationApplier where T : struct,IComponent
    {
        protected readonly Stash<T> Stash;
        protected abstract T InitialValue { get; }

        [Inject]
        public HexCreationComponentApplier(World world)
        {
            Stash = world.GetStash<T>();
        }
        public void ApplyOnCreate(Entity hexEntity)
        {
            Stash.Add(hexEntity, InitialValue);
        }
    }
}
