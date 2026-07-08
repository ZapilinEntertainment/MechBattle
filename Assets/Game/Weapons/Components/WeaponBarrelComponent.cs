using Scellecs.Morpeh;
using UnityEngine;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct WeaponBarrelComponent : IComponent 
    {

        public float2 RadianRotation; 
        public readonly float2 RadianRotationSpeed;
        public readonly bool YRotationPossible => RadianRotationSpeed.y != 0f;

        /// <summary>
        /// NOTE: barrel have 2 options - rotation only by X (standart artillery tower) or rotating both X and Y (sentry gun)
        /// It cannot be rotated Y only.
        /// </summary>
        public WeaponBarrelComponent(float radianRotationSpeedX, float radianRotationSpeedY)
        {
            RadianRotationSpeed = new float2(radianRotationSpeedX, radianRotationSpeedY);
            RadianRotation = float2.zero;
        }
    
    }
}