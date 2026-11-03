using Scellecs.Morpeh;
using System;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Weapons
{
    public class WeaponEnergyCostFeatureApplier : IWeaponFeatureApplier
    {
        public WeaponFeatureFlags RequiredFlags => WeaponFeatureFlags.EnergyConsumption;
        public WeaponFeatureApplyOrder ApplyOrder => WeaponFeatureApplyOrder.Other;
        private readonly Stash<WeaponDischargeEnergyConsumption> _dischargeEnergyCosts;
        private readonly Stash<WeaponChargeEnergyConsumption> _chargeEnergyCosts;
        private readonly Stash<ShotEnergyCostComponent> _shotEnergyCosts;

        [Inject]
        public WeaponEnergyCostFeatureApplier(World world)
        {
            _dischargeEnergyCosts = world.GetStash<WeaponDischargeEnergyConsumption>();
            _chargeEnergyCosts = world.GetStash<WeaponChargeEnergyConsumption>();
            _shotEnergyCosts = world.GetStash<ShotEnergyCostComponent>();
        }

        public void Apply(ref WeaponConstructionProtocol constructionProtocol)
        {
            if (!constructionProtocol.WeaponConfig.TryGetFeatureConfig<WeaponEnergyCostConfig>(out var config))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError("no energy consumption config defined");
#endif
                return;
            }

            if (config.DischargeCost != 0f)
                _dischargeEnergyCosts.Add(constructionProtocol.WeaponEntity, new(config.DischargeCost));

            if (config.ChargeCost != 0f)
                _chargeEnergyCosts.Add(constructionProtocol.WeaponEntity, new(config.ChargeCost));

            if (config.ShotCost != 0f)
                _shotEnergyCosts.Add(constructionProtocol.WeaponEntity, new(config.ShotCost));
        }
    }

    [Serializable]
    public class WeaponEnergyCostConfig : IWeaponFeatureConfig
    {
        [field: SerializeField] public float DischargeCost { get; private set; }
        [field: SerializeField] public float ChargeCost { get; private set; }
        [field: SerializeField] public float ShotCost { get; private set; }
    }
}
