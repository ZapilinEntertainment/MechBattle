using Scellecs.Morpeh;
using ZE.MechBattle.Units;

namespace ZE.MechBattle
{
    public interface IUnitFeatureApplier
    {
        void ApplyUnitFeature(Entity entity, IUnitConfig config);
        bool TryCacheApplier(IUnitConfig config, out ICachedApplier position);


    }

    public abstract class UnitFeatureApplierBase<T> : IUnitFeatureApplier where T : IUnitFeatureConfig
    {

        public void ApplyUnitFeature(Entity entity, IUnitConfig unitConfig)
        {
            if (!unitConfig.TryGetFeaturedConfig<T>(out var featureConfig))
                return;

            ApplyUnitFeature(entity, featureConfig);
        }


        public abstract void ApplyUnitFeature(Entity entity, T config);

        public abstract bool TryCacheApplier(IUnitConfig config, out ICachedApplier position);
    }
}
