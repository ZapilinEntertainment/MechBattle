using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public interface IHexPathsList
    {
        bool TryGetPath(int pathId, out PathData<int> data);
    }

    public class HexPathsLRUBuffer : UserCountDependentLRUPathsBuffer<Entity, int>, IHexPathsList
    {
        

    }
}
