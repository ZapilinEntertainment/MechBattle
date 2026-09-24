using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Navigation.DebugOverlay;

namespace ZE.MechBattle.Develop
{
    public class MovementDensityValuesDrawer : MonoBehaviour
    {
        [SerializeField] private Gradient _gradient;
        [SerializeField] private float _maxDensity = 10f;

        private bool _isReady = false;
        private float _triangleHeight;
        private IMovementDensityMap _densityMap;
        private INavigationMap _navigationMap;
        private Filter _filter;
        private Stash<CellMovementDensityComponent> _density;
        private Stash<CellEntityComponent> _cells;

        [Inject]
        public void Inject(IMovementDensityMap densityMap, World world, INavigationMap navigationMap)
        {
            _densityMap = densityMap;
            _navigationMap = navigationMap;
            _triangleHeight = navigationMap.TriangleHeight;

            _filter = world.Filter
                .With<CellMovementDensityComponent>()
                .Build();

            _density = world.GetStash<CellMovementDensityComponent>();
            _cells = world.GetStash<CellEntityComponent>();

            _isReady = true;
        }

        private void OnDrawGizmos()
        {
            if (isActiveAndEnabled & _isReady)
            {
                foreach (var cellEntity in _filter)
                {
                    var density = _density.Get(cellEntity).Value;
                    if (density == 0f)
                        continue;

                    var densityPc = math.clamp(density / _maxDensity, 0f, 1f);
                    Handles.color = _gradient.Evaluate(densityPc);

                    var tripos = _cells.Get(cellEntity).Tripos;
                    TrianglesDrawHelper.DrawHandles(vertices: TrianglesDrawHelper.GetDrawVertices(tripos, _navigationMap), opaque: true);
                }
            }
        }
    }
}
