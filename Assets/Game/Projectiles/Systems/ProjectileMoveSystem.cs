using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]

    // TODO: add pause handling
    public sealed class ProjectileMoveSystem : IFixedSystem 
    {

        public World World { get; set;}
        private Filter _filter;
        private Stash<SpeedComponent> _speed;
        private Stash<ExplosionTimerComponent> _explosionTimer;
        private Stash<ExplodeTag> _explodeTags;
        private Stash<CollisionComponent> _collisionResults;
        private TransformAspectHandler _transformAspect;

        private readonly List<float3> _movementVectorsCache = new (DEFAULT_CAPACITY);
        private readonly List<Entity> _projectilesList = new(DEFAULT_CAPACITY);
        private readonly QueryParameters _queryParameters;
        private const int DEFAULT_CAPACITY = 32;

        [Inject]
        public ProjectileMoveSystem()
        {
            _queryParameters = new QueryParameters()
            {
                hitBackfaces = false,
                hitMultipleFaces = false,
                hitTriggers = QueryTriggerInteraction.Ignore,
                layerMask = LayerConstants.ProjectilesCastMask
            };
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<ProjectileComponent>()
                .With<SpeedComponent>()
                .Without<ExplodeTag>()
                .Build();

            _speed = World.GetStash<SpeedComponent>();
            _explosionTimer = World.GetStash<ExplosionTimerComponent>();
            _explodeTags = World.GetStash<ExplodeTag>();
            _collisionResults = World.GetStash<CollisionComponent>();

            _transformAspect = new(World);
        }

        public void OnUpdate(float dt)
        {
            if (_filter.IsNotEmpty())
            {
                var count = 0;
                foreach (var projectile in _filter)
                {
                    ref var explosionTimer = ref _explosionTimer.Get(projectile);
                    explosionTimer.Value -= dt;
                    if (explosionTimer.Value <= 0)
                    {
                        _explodeTags.Add(projectile);
                    }
                    else
                    {
                        _projectilesList.Add(projectile);
                        count++;
                    }
                }

                if (count != 0)
                {
                    // TODO: do not use jobs when only few projectiles exists

                    var raycastCommands = new NativeArray<RaycastCommand>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                    for (var i = 0; i < count; i++)
                    {
                        var projectile = _projectilesList[i];
                        var position = _transformAspect.GetPosition(projectile);
                        var direction = _transformAspect.GetForward(projectile);
                        var step = _speed.Get(projectile).Value * dt;
                        raycastCommands[i] = new RaycastCommand(position, direction, _queryParameters, step);
                        _movementVectorsCache.Add(step * direction);
                    }

                    var results = new NativeArray<RaycastHit>(2 * count, Allocator.TempJob);
                    var handle = RaycastCommand.ScheduleBatch(raycastCommands, results, 16);
                    handle.Complete();                                    

                    for (var i = 0; i < count; i++)
                    {
                        var result = results[i];
                        var projectile = _projectilesList[i];

                        if (result.collider != null)
                        {
                            _collisionResults.Set(projectile, new() { Result = new(result.colliderInstanceID, result.normal) });
                            _explodeTags.Add(projectile);
                        }
                        else
                        {
                            _transformAspect.Translate(projectile, _movementVectorsCache[i], Space.World);
                        }
                    }

                    raycastCommands.Dispose();
                    results.Dispose();
                    _movementVectorsCache.Clear();
                }

                _projectilesList.Clear();
            }
        }

        public void Dispose()
        {
            _projectilesList.Clear();
            _movementVectorsCache.Clear();
        }
    }
}