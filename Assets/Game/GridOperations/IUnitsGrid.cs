using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public interface IUnitsGrid
    {
        bool TryGetUnitsInHex(int2 hexCoord, out IReadOnlyList<Entity> entitiesList);

    }
}
