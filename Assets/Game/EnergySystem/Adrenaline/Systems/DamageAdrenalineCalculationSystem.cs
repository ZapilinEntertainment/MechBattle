using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using System.Collections.Generic;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class DamageAdrenalineCalculationSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _adrenalineFilter;
        private Filter _partitionsFilter;
        private Stash<MechPartitionComponent> _partitions;
        private Stash<DamageAdrenalineComponent> _damageAdrenalineComponent;
        private Stash<AdrenalineComponent> _adrenaline;

        private readonly IReceivedDamageList _receivedDamageList;
        private readonly Dictionary<Entity, float> _compositeEntitiesDamage = new();

        [Inject]
        public DamageAdrenalineCalculationSystem(IReceivedDamageList receivedDamageList)
        {
            _receivedDamageList = receivedDamageList;
        }

        public void OnAwake() 
        {
            _adrenalineFilter = World.Filter.With<AdrenalineComponent>().Build();
            _partitionsFilter = World.Filter
                .With<MechPartitionComponent>()
                .With<DamageReceivedComponent>()
                .Build();

            _partitions = World.GetStash<MechPartitionComponent>();
            _damageAdrenalineComponent = World.GetStash<DamageAdrenalineComponent>();
            _adrenaline = World.GetStash<AdrenalineComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_adrenalineFilter.IsEmpty())
                return;

            // calculate all incoming damage for each mech
            _compositeEntitiesDamage.Clear();
            foreach (var partitionEntity in _partitionsFilter)
            {
                if (!_receivedDamageList.TryGetDamageData(partitionEntity, out var damageData))
                    continue;

                var mechEntity = _partitions.Get(partitionEntity).MechEntity;
                if (!_compositeEntitiesDamage.TryGetValue(mechEntity, out var totalDamageVolume))
                {
                    _compositeEntitiesDamage.Add(mechEntity, damageData.Volume);
                }
                else
                {
                    _compositeEntitiesDamage[mechEntity] = totalDamageVolume + damageData.Volume;
                }
            }

            // check all adrenaline users
            foreach (var entity in _adrenalineFilter)
            {
                var damage = 0f;
                if (_receivedDamageList.TryGetDamageData(entity, out var damageData))
                    damage += damageData.Volume;

                if (_compositeEntitiesDamage.TryGetValue(entity, out var compositeDamageVolume))
                    damage += compositeDamageVolume;

                var adrenaline = _adrenaline.Get(entity);

                _damageAdrenalineComponent.Set(entity, new() { AdrenalineVolume = damage * adrenaline.AdrenalineDamageReceiveCf });
            }
        }

        public void Dispose() { }
    }
}