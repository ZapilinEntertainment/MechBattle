using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;


namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct WeaponLoadingComponent : IIntervalUpdateComponent
    {
        public float TimeLeft { get;set; }

        public float Interval => _interval;
        public float LoadingPc => math.clamp(1f - TimeLeft / _interval, 0f, 1f);
        private readonly float _interval;

        public WeaponLoadingComponent(float interval)
        {
            _interval = interval;
            TimeLeft = _interval;
        }
    }
}