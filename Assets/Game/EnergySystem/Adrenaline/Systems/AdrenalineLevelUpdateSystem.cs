using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class AdrenalineLevelUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<AdrenalineComponent> _adrenaline;
        private Stash<DamageAdrenalineComponent> _damageAdrenaline;
        private Stash<EnergySpentAdrenalineComponent> _energySpentAdrenaline;

        public void OnAwake() 
        {
            _filter = World.Filter.With<AdrenalineComponent>().Build();

            _adrenaline = World.GetStash<AdrenalineComponent>();
            _damageAdrenaline = World.GetStash<DamageAdrenalineComponent>();
            _energySpentAdrenaline = World.GetStash<EnergySpentAdrenalineComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            foreach (var entity in _filter)
            {
                var damageAdrenalineComponent = _damageAdrenaline.Get(entity, out var hasDamageAdrenaline);
                var energySpentAdrenalineComponent = _energySpentAdrenaline.Get(entity, out var hasEnergySpendAdrenaline);

                var damageAdrenaline = hasDamageAdrenaline ? damageAdrenalineComponent.AdrenalineVolume : 0f;
                var energySpentAdrenaline = hasEnergySpendAdrenaline ? energySpentAdrenalineComponent.AdrenalineVolume : 0f;

                var resultingAdrenaline = damageAdrenaline + energySpentAdrenaline;
                if (resultingAdrenaline == 0f)
                {
                    LowerAdrenaline(entity, deltaTime);
                }                    
                else
                {
                    RaiseAdrenaline(entity, resultingAdrenaline);
                    //UnityEngine.Debug.Log($"{entity.Id} : {damageAdrenaline} : {energySpentAdrenaline}");
                }
                    
            }

            _damageAdrenaline.RemoveAll();
            _energySpentAdrenaline.RemoveAll();
        }

        public void Dispose() { }

        private void LowerAdrenaline(Entity entity, float deltaTime)
        {
            ref var adrenaline = ref _adrenaline.Get(entity);
            adrenaline.AdrenalineVolume = math.clamp(adrenaline.AdrenalineVolume - adrenaline.AdrenalineFallbackSpeed * deltaTime, 0f, adrenaline.MaxAdrenalineVolume);
            adrenaline.CurrentAdrenalineLevel = (int)(adrenaline.AdrenalineVolume / adrenaline.AdrenalineLevelLimit);

            //UnityEngine.Debug.Log($"↓ {entity.Id} : {adrenaline.AdrenalineVolume} / {adrenaline.CurrentAdrenalineLevel}");
        }

        private void RaiseAdrenaline(Entity entity, float deltaVolume)
        {
            ref var adrenaline = ref _adrenaline.Get(entity);
            var volume = adrenaline.AdrenalineVolume + deltaVolume;
            adrenaline.AdrenalineVolume = math.clamp(volume, 0f, adrenaline.MaxAdrenalineVolume);
            adrenaline.CurrentAdrenalineLevel = (int)(adrenaline.AdrenalineVolume / adrenaline.AdrenalineLevelLimit);

            //UnityEngine.Debug.Log($"↑ {entity.Id} : {adrenaline.AdrenalineVolume} / {adrenaline.CurrentAdrenalineLevel}");
        }
    }
}