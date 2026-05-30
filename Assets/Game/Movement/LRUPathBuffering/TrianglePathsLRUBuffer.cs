using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class TrianglePathsLRUBuffer : 
        UserCountDependentLRUPathsBuffer<IntTriangularPos, IntTriangularPos, TrianglesPath>, 
        IPathStorage<TrianglesPath>
    {
        public bool TryGetPathById(int pathId, out TrianglesPath path) => TryGetValue(pathId, out path, updateUsingTime: true);

        protected override TrianglesPath CreateNewPath(int pathId, IntTriangularPos start, IntTriangularPos end) =>
            new(pathId, (start, end));
    }
}
