using Unity.Mathematics;
using UnityEngine;
using TriInspector;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Views;
using System.Collections.Generic;

namespace ZE.MechBattle.Develop
{
    public class VirtualViewsDebugDrawer : MonoBehaviour
    {
        [Range(0,1f), SerializeField] private float _spawnChance = 1f;
        [SerializeField] private int _maxObjectsCountPerTriangle = 3;
        [SerializeField] private float _minSize = 1f;
        [SerializeField] private float _maxSize = 5f;
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;
        [ShowInInspector, ReadOnly] private int _totalCount;

        private bool _isInjected = false;
        private bool _draw = false;
        private IEntitiesNavigationMap _navigationMap;
        private Filter _filter;
        private Stash<CellEntityComponent> _cellEntities;
        private VirtualRenderer _virtualRenderer;
        private List<Matrix4x4> _matricesList = new();


        [Inject]
        public void Inject(IEntitiesNavigationMap navigationMap, World world, VirtualRenderer virtualRenderer)
        {
            _navigationMap = navigationMap;
            _filter = world.Filter.With<CellEntityComponent>().Build();
            _cellEntities = world.GetStash<CellEntityComponent>();
            _isInjected = true;

            _virtualRenderer = virtualRenderer;
        }

        [EnableInPlayMode, Button("Draw")]
        public void Draw()
        {
            if (!_isInjected)
            {
                UnityEngine.Debug.LogError("not injected");
                return;
            }

            _matricesList.Clear();
            _totalCount = 0;
            _draw = false;

            var height = _navigationMap.TriangleHeight;
            foreach (var cell in _filter)
            {
                var tripos = _cellEntities.Get(cell).Tripos.ToFloat3();
                for (var i = 0; i < _maxObjectsCountPerTriangle; i++)
                {
                    if (UnityEngine.Random.value >= _spawnChance)
                        continue;

                    var normalizedInnerTripos = GetRandomTriaxialCoordinate();
                    var worldPos = TriangularMath.TriangularToWorld(normalizedInnerTripos+ tripos, height);
                    var matrix = Matrix4x4.TRS(
                        worldPos, 
                        UnityEngine.Random.rotation, 
                        new Vector3(
                            UnityEngine.Random.Range(_minSize, _maxSize),
                            UnityEngine.Random.Range(_minSize, _maxSize),
                            UnityEngine.Random.Range(_minSize, _maxSize)));
                    _matricesList.Add(matrix);
                    _totalCount++;
                }
            }

            _draw = _totalCount != 0;     
        }

        private void Update()
        {
            if (_draw)
                _virtualRenderer.DrawMatrices(_mesh, _material, _matricesList);


        }

        private float3 GetRandomTriaxialCoordinate()
        {
            var a = UnityEngine.Random.value;
            var b = UnityEngine.Random.value;
            var c = UnityEngine.Random.value;
            var s = a + b + c;
            return new(a / s, b / s, c / s);
        }
    }
}
