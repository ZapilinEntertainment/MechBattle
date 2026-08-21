using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.MechMovement;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MechStepsMapClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private readonly IMechStepsAffectionMapSource _sourceMap;

        [Inject]
        public MechStepsMapClearSystem(IMechStepsAffectionMapSource source)
        {
            _sourceMap = source;
        }

        public void OnAwake() { }

        public void OnUpdate(float deltaTime) 
        {
            if (!_sourceMap.IsAffectionMapEmpty)
                _sourceMap.ClearData();
        }

        public void Dispose() { }
    }
}