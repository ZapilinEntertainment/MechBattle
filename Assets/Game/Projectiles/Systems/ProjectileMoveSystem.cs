using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using System;
using Unity.Jobs;

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
        private Stash<TransformComponent> _transforms;
        private Stash<CollisionComponent> _collisionResults;

        private readonly List<Entity> _projectilesList = new(32);
        private readonly QueryParameters _queryParameters;
        private const int MAX_PROJECTILES_PER_FRAME = 4096;

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
            _transforms = World.GetStash<TransformComponent>();
            _collisionResults = World.GetStash<CollisionComponent>();
            Debug.Log("move system awake");
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

                // excessive defence measure (stackalloc). Note that list can be longer
                count = math.clamp(count, 0, MAX_PROJECTILES_PER_FRAME);

                if (count != 0)
                {
                    // TODO: do not use jobs when only few projectiles exists

                    var raycastCommands = new NativeArray<RaycastCommand>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                    Span<float> stepsBuffer = stackalloc float[count];

                    for (var i = 0; i < count; i++)
                    {
                        var projectile = _projectilesList[i];
                        var transform = _transforms.Get(projectile);
                        var position = transform.Position;
                        var direction = transform.Forward;
                        var step = _speed.Get(projectile).Value * dt;
                        raycastCommands[i] = new RaycastCommand(position, direction, _queryParameters, step);
                        stepsBuffer[i] = step;
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
                            _transforms.Get(projectile).Value.Translate(new Vector3(0f, 0f, stepsBuffer[i]), Space.Self);
                        }
                    }

                    raycastCommands.Dispose();
                    results.Dispose();
                }

                _projectilesList.Clear();
            }
        }

        public void Dispose()
        {
            _projectilesList.Clear();
        }
    }
}