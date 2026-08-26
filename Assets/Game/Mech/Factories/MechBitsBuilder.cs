using Scellecs.Morpeh;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.MechBuilding
{
    public class MechBitsBuilder
    {
        public IReadOnlyDictionary<ViewPartKey, Entity> ConstructedParts => _constructedParts;

        private MechConfig _mechConfig;
        private Entity _mechEntity;
        private ICollection<ViewPartKey> _separatingKeys;

        private readonly World _world;
        private readonly Dictionary<ViewPartKey, Entity> _constructedParts = new();
        private readonly Dictionary<ViewPartKey, MechPartSettings> _settings = new();
        private readonly HashSet<ViewPartKey> _processingPartsSet = new();
        private readonly ParentingRelationsApplier _parentingRelationsApplier;

        private readonly Stash<RotationSpeedComponent> _rotationSpeed;
        private readonly Stash<LocalRotationLimitComponent> _localRotationLimits;
        

        [Inject]
        public MechBitsBuilder(ParentingRelationsApplier parentingRelationsApplier, World world)
        {
            _parentingRelationsApplier = parentingRelationsApplier;
            _world = world;

            _rotationSpeed = _world.GetStash<RotationSpeedComponent>();
            _localRotationLimits = _world.GetStash<LocalRotationLimitComponent>();
        }

        public void BuildParts(Entity mechEntity, MechConfig mechConfig, ICollection<ViewPartKey> separatingKeys)
        {
            _mechConfig = mechConfig;
            _mechEntity = mechEntity;
            _separatingKeys = separatingKeys;

            foreach (var mechPartSettings in _mechConfig.MechPartSettings)
            {
                var key = mechPartSettings.Key;
                if (!_settings.TryAdd(key, mechPartSettings))
                {
                    UnityEngine.Debug.LogWarning("key duplication: " + key);
                    continue;
                }
            }

            foreach (var mechPartKey in _settings.Keys)
            {
                TryBuildPart(mechPartKey, out _);
            }
        }

        public void AddConstructedPart(ViewPartKey key, Entity entity)
        {
            if (!_constructedParts.TryAdd(key, entity))
                UnityEngine.Debug.LogError("cannot add " + key.ToString());
        }

        public bool TryGetConstructedPartEntity(ViewPartKey key, out Entity entity) =>
            _constructedParts.TryGetValue(key, out entity);

        public IEnumerator<KeyValuePair<ViewPartKey, Entity>> GetEnumerator() => _constructedParts.GetEnumerator();

        private bool TryBuildPart(ViewPartKey key, out Entity entity)
        {
            entity = default;

            var settings = _settings[key];
            if (settings.ConstructionMode == MechPartConstructionMode.DoNothing)
            {
                return false;
            }

            if (_constructedParts.TryGetValue(key, out entity))
            {
                UnityEngine.Debug.LogWarning("key duplication: " + key.ToString());
                return true;
            }

            _processingPartsSet.Clear();

            var rootKey = settings.RootKey;
            Entity parentEntity;
            if (!rootKey.IsValid)
            {
                parentEntity = _mechEntity;
            }
            else
            {
                if (!_constructedParts.TryGetValue(rootKey, out parentEntity))
                {
                    _processingPartsSet.Add(key);

                    if (_processingPartsSet.Contains(rootKey))
                    {
                        UnityEngine.Debug.LogError("circular error with root " + rootKey);
                        return false;
                    }

                    if (!TryBuildPart(rootKey, out parentEntity))
                    {
                        UnityEngine.Debug.LogError("root build failure: cannot find settings for " + rootKey);
                        return false;
                    }
                }
            }
            

            entity = BuildPart(key, settings, parentEntity, _mechEntity);
            AddConstructedPart(key, entity);
            return true;
        }

        private Entity BuildPart(ViewPartKey key, MechPartSettings settings, Entity parent, Entity viewOwner)
        {
            Entity entity;
            if (settings.ConstructionMode == MechPartConstructionMode.LinkToViewPart)
            {
                if (!key.IsValid)
                    throw new System.Exception("view part key invalid");

                entity = _parentingRelationsApplier.CreateChildEntityForViewPart(
                    settings.AttachProtocol.ToPoint(),
                    parent,
                    viewOwner,
                    key,
                    separateViewObject: _separatingKeys.Contains(key));
            }
            else
            {
                throw new System.NotImplementedException("construction mode not implemented");
            }

            _rotationSpeed.Set(entity, new(settings.RotationSpeedRadians));
            if (math.any(settings.RotationLimits.LimitAxleRotation))
                _localRotationLimits.Set(entity, new(settings.RotationLimits));

            return entity;
        }
    }
}
