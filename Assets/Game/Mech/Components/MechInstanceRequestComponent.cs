using Scellecs.Morpeh;
using UnityEngine;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs 
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct MechInstanceRequestComponent : IComponent 
    {
        public readonly PlayerKey PlayerKey;
        public readonly bool AssumingDirectControl;
        public readonly float3 Position;
        public readonly quaternion Rotation;

        public MechInstanceRequestComponent(PlayerKey playerKey, float3 position, quaternion rotation, bool directControl)
        {
            PlayerKey = playerKey;
            Position = position;
            Rotation = rotation;
            AssumingDirectControl = directControl;
        }
    
    }
}