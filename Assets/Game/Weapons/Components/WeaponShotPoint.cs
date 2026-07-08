using Scellecs.Morpeh;
using UnityEngine;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct WeaponShotPoint : IComponent 
    {
        public readonly float3 LocalPos;  
        public RigidTransform WorldPoint;

        public WeaponShotPoint(float3 localPos)
        {
            LocalPos = localPos;
            WorldPoint = default;
        }
    }
}