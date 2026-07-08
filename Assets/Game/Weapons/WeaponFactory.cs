using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle.Ecs
{
    public class WeaponFactory
    {
        private readonly World _world;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;
        private readonly StringDataDictionary _stringDictionary;
        private readonly Stash<WeaponRangeComponent> _ranges;
        private readonly Stash<DamageComponent> _damages;
        private readonly Stash<WeaponComponent> _weaponComponents;
        private readonly Stash<WeaponUpdateComponent> _weaponUpdateComponents;

        [Inject]
        public WeaponFactory(World world, ParentingRelationsApplier parentingRelationsApplier, StringDataDictionary stringDataDictionary)
        {
            _world = world;
            _parentingRelationsApplier = parentingRelationsApplier;
            _stringDictionary = stringDataDictionary;

            _ranges = _world.GetStash<WeaponRangeComponent>();
            _damages = _world.GetStash<DamageComponent>();
            _weaponComponents = world.GetStash<WeaponComponent>();
            _weaponUpdateComponents = world.GetStash<WeaponUpdateComponent>();
        }

        public Entity CreateUnitWeapon(Entity parentEntity, WeaponConfig weaponConfig, WeaponAttachmentProtocol attachmentProtocol)
        {
            var weaponEntity = _world.CreateEntity();
            _ranges.Add(weaponEntity, new(weaponConfig.MinRange, weaponConfig.MaxRange, weaponConfig.RecommendedRangePc));
            _damages.Add(weaponEntity, new() { DamageParameters = new() { Value = weaponConfig.Damage} });

            _parentingRelationsApplier.Apply(new()
            {
                ParentEntity = parentEntity,
                ChildEntity = weaponEntity,
                LocalPos = attachmentProtocol.LocalPosition,
                LocalRot = attachmentProtocol.LocalRotation
            });

            _weaponComponents.Add(weaponEntity, new(_stringDictionary.GetStringKey(weaponConfig.ProjectileId)));
            _weaponUpdateComponents.Add(weaponEntity, new(weaponConfig.Cooldown));

            return weaponEntity;
        }
    
    }
}
