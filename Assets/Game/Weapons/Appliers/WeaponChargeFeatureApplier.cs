using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Weapons
{
    public class WeaponChargeFeatureApplier : IWeaponFeatureApplier
    {
        public WeaponFeatureFlags RequiredFlags => WeaponFeatureFlags.Chargeable;
        public WeaponFeatureApplyOrder ApplyOrder => WeaponFeatureApplyOrder.Other;
        private readonly Stash<WeaponChargeComponent> _weaponChargeComponents;

        [Inject]
        public WeaponChargeFeatureApplier(World world)
        {
            _weaponChargeComponents = world.GetStash<WeaponChargeComponent>();
        }

       

        public void Apply(ref WeaponConstructionProtocol constructionProtocol)
        {
            if (!constructionProtocol.WeaponConfig.TryGetFeatureConfig<WeaponChargeFeatureConfig>(out var config))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError("no charge config defined");
#endif
                return;
            }

            _weaponChargeComponents.Add(constructionProtocol.WeaponEntity, new(config.ChargeSettings));
        }
    }
}
