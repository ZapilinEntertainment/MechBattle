using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public interface ITriangleNeighbourOffsets
    {
        int3 this[int index] { get; }
    }
}
