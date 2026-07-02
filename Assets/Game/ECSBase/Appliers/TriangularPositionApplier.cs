using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    public class TriangularPositionApplier
    {
        private readonly float _invertedTriangleHeight;
        private readonly Stash<TriangularPosComponent> _stash;

        [Inject]
        public TriangularPositionApplier(INavigationMap map, World world)
        {
            _stash = world.GetStash<TriangularPosComponent>();
            _invertedTriangleHeight = map.InvertedTriangleHeight;
        }

        public void Apply(Entity entity, float3 worldPos)
        {
            var tripos = TriangularMath.WorldToTrianglePosInvertedHeight(worldPos, _invertedTriangleHeight);
            _stash.Set(entity, new() { Value = tripos });
        }
    
    }
}
