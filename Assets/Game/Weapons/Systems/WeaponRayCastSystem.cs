using Scellecs.Morpeh;
using System;
using System.Buffers;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponRayCastSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<WeaponShotPoint> _shotPoints;

        private readonly DamageRequestsFactory _damageRequestsFactory;
        private readonly WeaponHandler _weaponHandler;
        private readonly WeaponRaycasterFactory _raycasterFactory;
        private readonly Dictionary<Entity, IWeaponRayCaster> _activeRaycasters = new();

        [Inject]
        public WeaponRayCastSystem(DamageRequestsFactory damageRequestsFactory, WeaponHandler weaponHandler, WeaponRaycasterFactory weaponRaycasterFactory)
        {
            _damageRequestsFactory = damageRequestsFactory;
            _weaponHandler = weaponHandler;
            _raycasterFactory = weaponRaycasterFactory;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<WeaponRayComponent>()
                .With<WeaponFireTag>()
                .Build();

            _shotPoints = World.GetStash<WeaponShotPoint>();
        }

        public void OnUpdate(float deltaTime) 
        {
            var currentFrameIndex = Time.frameCount;
            // update active casters (increase frame index in those who met filter)
            foreach (var weaponEntity in _filter)
            {
                if (!_activeRaycasters.TryGetValue(weaponEntity, out var raycaster))
                {
                    raycaster = AddNewRaycaster(weaponEntity);
                    _activeRaycasters.Add(weaponEntity, raycaster);
                    //UnityEngine.Debug.Log($"add caster for entity {weaponEntity.Id}");
                }
                raycaster.UpdateFrameIndex(currentFrameIndex);
            }

            // clear outdated casters (from cached list)
            var activeCastersCount = _activeRaycasters.Count;
            if (activeCastersCount != 0)
            {
                Span<Entity> clearList = stackalloc Entity[activeCastersCount];
                var index = 0;
                foreach (var raycasterKvp in _activeRaycasters)
                {
                    if (raycasterKvp.Value.IsOutdated(currentFrameIndex))
                    {
                        clearList[index++] = raycasterKvp.Key;
                        raycasterKvp.Value.Dispose();
                    }  
                }

                if (index != 0)
                {
                    for (var i = 0; i < index; i++)
                    {
                        _activeRaycasters.Remove(clearList[i]);
                        activeCastersCount--;
                    }
                }
            }

            if (activeCastersCount == 0)
                return;

            // calculate end points and apply damage
            foreach (var casterKvp in _activeRaycasters)
            {
                var startPoint = _shotPoints.Get(casterKvp.Key).WorldPoint;
                var ray = new Ray(startPoint.pos, math.forward(startPoint.rot));
                //UnityEngine.Debug.Log(startPoint.pos);

                var caster = casterKvp.Value;
                var maxDistance = caster.MaxCastDistance;
                Vector3 endPos;
                var objectHit = Physics.Raycast(ray, out var raycastHit, maxDistance, LayerConstants.RayWeaponCastMask, QueryTriggerInteraction.Ignore);
                if (objectHit)
                {
                    endPos = raycastHit.point;
                    ApplyDamage(casterKvp.Key, raycastHit.colliderInstanceID, caster.CalculateCurrentDamageCf());
                }
                else
                {
                    endPos = ray.GetPoint(maxDistance);
                }

                caster.UpdateEndPoints(startPoint.pos, endPos, objectHit);
            }
        }

        public void Dispose()
        {
            foreach (var raycaster in _activeRaycasters.Values)
            {
                raycaster.Dispose();
            }
            _activeRaycasters.Clear();
        }

        private IWeaponRayCaster AddNewRaycaster(Entity weaponEntity) => _raycasterFactory.Create(weaponEntity);

        private void ApplyDamage(Entity weaponEntity, int targetColliderId, float rayDamageDistanceCf)
        {
            var damageParameters = _weaponHandler.GetWeaponDamage(weaponEntity);
            damageParameters = damageParameters.Multiply(rayDamageDistanceCf);
            _damageRequestsFactory.Build(weaponEntity, targetColliderId, damageParameters);
        }
    }
}