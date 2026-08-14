using System.Collections.Generic;
using VContainer;

namespace ZE.MechBattle
{
    public class CompositeResourceBinder : IResourceBinder
    {
        private readonly List<IResourceBinder> _binders = new();

        public void Add(IResourceBinder binder) => _binders.Add(binder);

        public void Register(IContainerBuilder builder)
        {
            foreach (var binder in _binders)
                binder.Register(builder);
        }
    }
}
