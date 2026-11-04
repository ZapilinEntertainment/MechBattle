using Scellecs.Morpeh;

namespace ZE.MechBattle.Units
{
    public interface ICachedApplier
    {
        void Apply(Entity unitEntity);
    }

    public class SpecificatedConfigCachedApplier<ApplierType, ConfigType> : ICachedApplier
        where ApplierType : UnitFeatureApplierBase<ConfigType>
        where ConfigType : IUnitFeatureConfig
    {
        private readonly ApplierType _applier;
        private readonly ConfigType _config;

        public SpecificatedConfigCachedApplier(ApplierType applier, ConfigType specificatedConfig)
        {
            _applier = applier;
            _config = specificatedConfig;
        }

        public void Apply(Entity unitEntity) => _applier.ApplyUnitFeature(unitEntity, _config);
    }


    // for appliers that don't use specialized configs
    public class BaseConfigCachedApplier : ICachedApplier
    {
        private readonly IUnitFeatureApplier _applier;
        private readonly IUnitConfig _config;

        public BaseConfigCachedApplier(IUnitFeatureApplier applier, IUnitConfig baseConfig)
        {
            _applier = applier;
            _config = baseConfig;
        }

        public void Apply(Entity unitEntity) => _applier.ApplyUnitFeature(unitEntity, _config);
    }
}
