using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public interface IEntitiesMap
    {
        bool TryGetEntity(IntTriangularPos tripos, out Entity entity);
    
    }
}
