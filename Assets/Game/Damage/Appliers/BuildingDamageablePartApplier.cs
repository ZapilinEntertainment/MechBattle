using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class BuildingDamageablePartApplier : BuildingFeatureApplierBase<BuildingDamageableMonoComponent>
    {
        public override BuildingFunctionFlags RequiredFlagsMask => BuildingFunctionFlags.Destructible;
        private readonly Stash<HealthComponent> _health;
        private readonly Stash<ViewDestroyEffectComponent> _viewDestroyEffect;
        private readonly Stash<TargetDataComponent> _targetDataComponents;
        private readonly StringDataDictionary _stringDataDictionary;

        [Inject]
        public BuildingDamageablePartApplier(World world, StringDataDictionary stringDataDictionary)
        {
            _stringDataDictionary = stringDataDictionary;

            _health = world.GetStash<HealthComponent>();
            _viewDestroyEffect = world.GetStash<ViewDestroyEffectComponent>();
            _targetDataComponents = world.GetStash<TargetDataComponent>();
        }

        protected override void ApplyBuildingFeature(Entity buildingEntity, BuildingDamageableMonoComponent damageableData)
        {
            _health.Add(buildingEntity, new(damageableData.HealthPoints));
            if (damageableData.IsAttackTarget)
                _targetDataComponents.Add(buildingEntity);

            var destructionProtocol = damageableData.DestructionProtocol;
            if (destructionProtocol.IsValid)
            {
                var destroyEffectKey = destructionProtocol.EffectId;
                if (!string.IsNullOrEmpty(destroyEffectKey))
                {
                    var encodedKey = _stringDataDictionary.StringToKey(destroyEffectKey);
                    _viewDestroyEffect.Set(buildingEntity, new() { EffectKey = encodedKey });
                }

              
            }
           
        }
    }
}
