using Scellecs.Morpeh;
using System.Collections.Generic;

namespace ZE.MechBattle.Units
{
    public class CachedUnitCompositeApplier
    {
        private readonly List<ICachedApplier> _cachedAppliers = new(8);

        public void Add(ICachedApplier cachedApplier) => _cachedAppliers.Add(cachedApplier);

        public void Apply(Entity unitEntity)
        {
            foreach (var applier in _cachedAppliers)
            {
                applier.Apply(unitEntity);
            }
        }

    }
}
