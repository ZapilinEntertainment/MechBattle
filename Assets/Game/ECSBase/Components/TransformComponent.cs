using Scellecs.Morpeh;
using UnityEngine;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs { 
    [System.Serializable]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct TransformComponent : IComponent 
    {
        public Transform Value;
        public float3 Position => Value.position;
        public float3 Forward => Value.forward;
        public quaternion Rotation => Value.rotation;
        public RigidTransform ToPoint() => Value.ToRigidTransform();
    }
}