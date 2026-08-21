using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.MechMovement {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MechStepsMapUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private readonly IMechStepsAffectionMapSource _source;
        private readonly IMechStepsMap _map;

        [Inject]
        public MechStepsMapUpdateSystem(IMechStepsAffectionMapSource source, IMechStepsMap map)
        {
            // - why we can't use data directly from source?
            // because it is job-used native list, we shouldn't copy to any another receivers
            // or a memory dispose problems appear

            _map = map;
            _source = source;
        }


        public void OnAwake() { }

        public void OnUpdate(float deltaTime) 
        {
            if (_source.IsAffectionMapEmpty)
                return;

            _map.Update(_source);
        }

        public void Dispose() { }
    }
}