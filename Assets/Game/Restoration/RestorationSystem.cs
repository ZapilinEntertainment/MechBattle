using UnityEngine;
using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RestorationSystem : ILateSystem 
    {
        public World World { get; set;}
        private Filter _restorablesFilter;
        private Stash<RestorableComponent> _restorablesStash;
        private readonly RestorablesList _restorablesList;

        [Inject]
        public RestorationSystem(RestorablesList list)
        {
            _restorablesList = list;
        }

        public void OnAwake() 
        {
            _restorablesFilter = World.Filter.With<RestorableComponent>().Build();
            _restorablesStash = World.GetStash<RestorableComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_restorablesFilter.IsNotEmpty())
            {
                var time = Time.time;
                foreach (var entity in _restorablesFilter)
                {
                    var component = _restorablesStash.Get(entity);
                    if (component.RestoreTime > time)
                        continue;

                    if (_restorablesList.TryGetElement(component.RestoreIndex, out var restorable))
                    {
                        restorable.Restore();
                    }

                    World.RemoveEntity(entity);
                }
            }
        }

        public void Dispose()
        {

        }
    }
}