using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponTargetPositionSetSystem : ISystem 
    {
        private readonly struct CachedResult
        {
            public readonly bool IsValid;
            public readonly float3 Position;

            public CachedResult(float3 pos)
            {
                Position = pos;
                IsValid = true;
            }
        }

        public World World { get; set;}
        private Filter _updateFilter;
        private Filter _clearFilter;
        private Stash<AttackTargetComponent> _attackTargets;
        private Stash<WeaponTargetPositionComponent> _targetPositions;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly Dictionary<Entity, CachedResult> _cachedFramePositions = new();

        [Inject]
        public WeaponTargetPositionSetSystem(TransformAspectHandler transformAspectHandler)
        {
            _transformAspectHandler = transformAspectHandler;
        }

        public void OnAwake() 
        {
            _updateFilter = World.Filter
                .With<AttackTargetComponent>()
                .Build();

            _clearFilter = World.Filter
                .With<WeaponTargetPositionComponent>()
                .Without<AttackTargetComponent>()
                .Build();

            _attackTargets = World.GetStash<AttackTargetComponent>();
            _targetPositions = World.GetStash<WeaponTargetPositionComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_updateFilter.IsNotEmpty())
            {
                foreach (var weaponEntity in _updateFilter)
                {
                    var targetEntity = _attackTargets.Get(weaponEntity).Entity;
                    CachedResult result = default;
                    if (!_cachedFramePositions.TryGetValue(targetEntity, out result))
                    {
                        float3 pos = default;
                        if (!World.IsDisposed(targetEntity) && _transformAspectHandler.TryGetPosition(targetEntity, out pos))
                            result = new(pos);

                        _cachedFramePositions.Add(targetEntity, result);
                    }

                    if (result.IsValid)
                        _targetPositions.Set(weaponEntity, new() { Value = result.Position });
                    else
                        _targetPositions.Remove(weaponEntity);
                }

                _cachedFramePositions.Clear();
            }

            

            foreach (var weaponEntity in _clearFilter)
            {
                _targetPositions.Remove(weaponEntity);
            }
        }

        public void Dispose() { }
    }
}