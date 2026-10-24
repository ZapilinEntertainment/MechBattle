using Scellecs.Morpeh;
using System.Collections.Generic;
using ZE.Utils;

namespace ZE.MechBattle
{
    public interface IUnitFeatureApplier
    {
        void ApplyUnitFeature(Entity entity, IUnitConfig config);
    
    }

    public abstract class UnitFeatureApplierBase<T> : IUnitFeatureApplier where T : IUnitFeatureConfig
    {
        private enum CachedResultMode : byte { Undefined, NoConfigPresented, CachedConfig}
        private readonly struct CachedResult
        {
            public readonly CachedResultMode Mode;
            public readonly T Config;

            public CachedResult(T value)
            {
                Mode = CachedResultMode.CachedConfig;
                Config = value;
            }

            private CachedResult(CachedResultMode mode)
            {
                Mode = mode;
                Config = default;
            }

            public static CachedResult NoConfigResult => new(CachedResultMode.NoConfigPresented);
        }

        private readonly Dictionary<IUnitConfig, CachedResult> _cache =
            new(ReferenceComparer<IUnitConfig>.Instance);

        public void ApplyUnitFeature(Entity entity, IUnitConfig unitConfig)
        {
            if (!_cache.TryGetValue(unitConfig, out CachedResult cachedResult))
            {
                if (!TryFindFeatureConfig(unitConfig, out var featureConfig))
                {
                    _cache.Add(unitConfig, CachedResult.NoConfigResult);
                    return;
                }

                cachedResult = new(featureConfig);
                _cache.Add(unitConfig, cachedResult);               
            }

            if (cachedResult.Mode == CachedResultMode.NoConfigPresented)
                return;

            ApplyUnitFeature(entity, cachedResult.Config);
        }
            

        protected abstract void ApplyUnitFeature(Entity entity, T config);

        private bool TryFindFeatureConfig(IUnitConfig unitConfig, out T result)
        {
            foreach (var config in unitConfig.FeatureConfigs)
            {
                if (config is T featureConfig)
                {
                    result = featureConfig;
                    return true;
                }
             }
            result = default;
            return false;
        }
    }
}
