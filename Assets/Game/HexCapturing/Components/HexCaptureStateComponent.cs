using Scellecs.Morpeh;
using System;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct HexCaptureStateComponent : IComponent 
    {
        public bool IsCaptured;
        public int ControllingPlayerId;
        public float CaptureProgress;

        public static HexCaptureStateComponent Empty => new()
        {
            IsCaptured = false,
            ControllingPlayerId = -1,
            CaptureProgress = 0f
        };
    }
}