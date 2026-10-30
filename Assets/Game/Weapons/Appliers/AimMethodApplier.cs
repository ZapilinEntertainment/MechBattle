using Scellecs.Morpeh;
using System.Collections.Generic;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class AimMethodApplier
    {
        private class AimTagStashes
        {
            private readonly IStash _calculateFirelineTags;
            private readonly IStash _aimTags;

            public AimTagStashes(IStash firelineCalculationTags, IStash aimTags)
            {
                _calculateFirelineTags = firelineCalculationTags;
                _aimTags = aimTags;
            }

            public void AddAll(Entity entity)
            {
                _calculateFirelineTags.Set(entity);
                _aimTags.Set(entity);
            }

            public void RemoveAll(Entity entity)
            {
                _calculateFirelineTags.Remove(entity);
                _aimTags.Remove(entity);
            }

            public void AddAimTag(Entity entity) => _aimTags.Set(entity);
            public void RemoveAimTag(Entity entity) => _aimTags.Remove(entity);
        }

        private readonly Dictionary<byte, AimTagStashes> _tagStashes = new();
        private readonly Stash<WeaponAimMethodComponent> _aimMethods;
        private readonly Stash<WeaponBarrelComponent> _barrels;

        [Inject]
        public AimMethodApplier(World world)
        {
            _aimMethods = world.GetStash<WeaponAimMethodComponent>();
            _barrels = world.GetStash<WeaponBarrelComponent>();

            var directAimStashes = new AimTagStashes(world.GetStash<CalculateFireLineByRaycastTag>(), world.GetStash<WeaponDirectAimTag>());
            _tagStashes.Add((byte)WeaponAimCheckMethod.DirectRaycast, directAimStashes);

            var highAimStashes = new AimTagStashes(world.GetStash<CalculateFireLineByHighArcTag>(), world.GetStash<WeaponHighAngleAimTag>());
            _tagStashes.Add((byte)WeaponAimCheckMethod.HighAngleRaycast, highAimStashes);
        }

        public void AddMethodTag(Entity weaponEntity)
        {
            var aimMethod = _aimMethods.Get(weaponEntity,out var aimExists);
            if (!aimExists)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"no aim method component at {weaponEntity.Id}");
#endif
                return;
            }

            var tagStash = _tagStashes[(byte)aimMethod.Value];
            tagStash.AddAll(weaponEntity);

            var barrelComponent = _barrels.Get(weaponEntity, out var haveBarrel);
            if (haveBarrel)
                tagStash.AddAimTag(barrelComponent.BarrelEntity);
        }

        public void RemoveMethodTag(Entity weaponEntity)
        {
            var aimMethod = _aimMethods.Get(weaponEntity, out var aimExists);
            if (!aimExists)
                return;

            var tagStash = _tagStashes[(byte)aimMethod.Value];
            tagStash.RemoveAll(weaponEntity);

            var barrelComponent = _barrels.Get(weaponEntity, out var haveBarrel);
            if (haveBarrel)
                tagStash.RemoveAimTag(barrelComponent.BarrelEntity);
        }    
    }
}
