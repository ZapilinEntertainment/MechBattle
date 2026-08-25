using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechBuilder
    {
        public struct PartData
        {
            public Entity Entity;
            //public List<string> SpecialKeywords;

            //public bool ContainsKeyword(string keyword) => SpecialKeywords != null && SpecialKeywords.Contains(keyword);
        }

        public Entity MechEntity;

        private readonly MechConfig _mechConfig;
        private readonly World _world;
        private readonly Dictionary<string, PartData> _constructedParts = new();
        private readonly HashSet<string> _processingPartsSet = new();
        private readonly ParentingRelationsApplier _parentingRelationsApplier;

        private readonly Stash<RotationSpeedComponent> _rotationSpeed;
        private readonly Stash<LocalRotationLimitComponent> _localRotationLimits;

        public MechBuilder(MechConfig mechConfig, ParentingRelationsApplier parentingRelationsApplier, World world)
        {
            _mechConfig = mechConfig;
            _parentingRelationsApplier = parentingRelationsApplier;
            _world = world;

            _rotationSpeed = _world.GetStash<RotationSpeedComponent>();
            _localRotationLimits = _world.GetStash<LocalRotationLimitComponent>();
        }

        public void AddConstructedPart(string key, PartData partData) => _constructedParts.Add(key, partData);

        public bool TryGetConstructedPartEntity(string key, out Entity entity)
        {
            if (_constructedParts.TryGetValue(key, out var partData))
            {
                entity = partData.Entity;
                return true;
            }
            else
            {
                entity = default;
                return false;
            }
        }

        public IEnumerator<KeyValuePair<string, PartData>> GetEnumerator() => _constructedParts.GetEnumerator();

        public bool TryBuildPart(string key)
        {
            if (_constructedParts.ContainsKey(key))
                return true;

            if (!_mechConfig.TryGetPartSettings(key, out var settings))
            {
                UnityEngine.Debug.LogError($"part {key} settings not found");
                return false;
            }

            _processingPartsSet.Clear();

            var rootId = settings.Root;
            Entity parentEntity;
            if (string.IsNullOrEmpty(rootId))
            {
                parentEntity = MechEntity;
            }
            else
            {
                if (!_constructedParts.TryGetValue(rootId, out var rootData))
                {
                    _processingPartsSet.Add(key);

                    if (_processingPartsSet.Contains(rootId))
                    {
                        UnityEngine.Debug.LogError("circular error with root " + rootId);
                        return false;
                    }

                    if (!TryBuildPart(rootId))
                    {
                        UnityEngine.Debug.LogError("root build failure: cannot find settings for " + rootId);
                        return false;
                    }
                    else
                    {
                        rootData = _constructedParts[rootId];
                    }
                }

                parentEntity = rootData.Entity;
            }
            

            var entity = BuildPart(key, settings, parentEntity);
            AddConstructedPart(key, new() { Entity = entity });
            return true;
        }

        private Entity BuildPart(string key, MechPartSettings settings, Entity parent)
        {
            Entity entity;
            var constructionProtocol = settings.ConstructProtocol;
            switch (constructionProtocol.ConstructionMode)
            {
                case ViewPartConstructionMode.EntityOnly:
                    {
                        entity = _world.CreateEntity();
                        break;
                    }
                case ViewPartConstructionMode.SyncWithViewPart:
                    {
                        if (!constructionProtocol.ViewPartKey.IsValid)
                            throw new System.Exception("view part key invalid");

                        entity = _parentingRelationsApplier.CreateChildEntityForViewPart(
                            settings.AttachProtocol.ToPoint(),
                            parent,
                            constructionProtocol.ViewPartKey);
                        break;
                    }
                default:
                    {
                        throw new System.NotImplementedException("construction mode not implemented");
                    }
            }

            _rotationSpeed.Set(entity, new(settings.RotationSpeedRadians));
            if (math.any(settings.RotationLimits.LimitAxleRotation))
                _localRotationLimits.Set(entity, new(settings.RotationLimits));

            return entity;
        }
    
    }
}
