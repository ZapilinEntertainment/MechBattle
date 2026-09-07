using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RepairDelaySetSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<RepairDelayComponent> _delays;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<RepairRequiredTag>()
                .With<DamageReceivedComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _delays = World.GetStash<RepairDelayComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                _delays.Set(entity, new(Time.time + MechConstants.DAMAGE_REPAIR_DELAY));
                //UnityEngine.Debug.Log($"repair delayed for entity {entity.Id} : {Time.time + MechConstants.DAMAGE_REPAIR_DELAY}");
            }
        }

        public void Dispose()
        {

        }
    }
}