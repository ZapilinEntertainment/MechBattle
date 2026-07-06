using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle.Ecs
{
    public class DisposeTagApplier
    {
        private readonly Stash<EntityDisposeTag> _disposeTag;

        [Inject]
        public DisposeTagApplier(World world)
        {
            _disposeTag = world.GetStash<EntityDisposeTag>();
        }

        // despite simplicity of this class it has 2 important functions:
        // 1. Dispose points overloop available
        // 2. Special dispose tactics (additional tags or components for specific dispose objects, ex.: replacing IDisposable interface on components)
        public void Apply(Entity entity) => _disposeTag.Set(entity);
    
    }
}
